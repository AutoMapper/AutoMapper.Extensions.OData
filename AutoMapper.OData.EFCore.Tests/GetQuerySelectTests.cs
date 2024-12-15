using AutoMapper.AspNet.OData;
using AutoMapper.OData.EFCore.Tests.Data;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.OData.EFCore.Tests
{
    public class GetQuerySelectTests : IClassFixture<GetQuerySelectTestsFixture>
    {
        private readonly GetQuerySelectTestsFixture _fixture;

        public GetQuerySelectTests(GetQuerySelectTestsFixture fixture)
        {
            _fixture = fixture;
            serviceProvider = _fixture.ServiceProvider;
        }

        #region Fields
        private readonly IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task OpsTenantSelectNameExpandBuildings()
        {
            string query = "/opstenant?$select=Name&$expand=Buildings&$orderby=Name";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
                Assert.Equal(default, collection.First().Identity);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsFilterEqAndOrderBy_FirstBuildingHasValues()
        {
            string query = "/opstenant?$top=5&$select=Buildings&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Name);
                Assert.NotEqual(default, collection.First().Buildings.First().Identity);
                Assert.Equal(default, collection.First().Identity);
                Assert.Null(collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingSelectNameExpandBuilder_BuilderNameShouldBeSam()
        {
            string query = "/corebuilding?$top=5&$select=Name&$expand=Builder($select=Name)&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Single(collection);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Null(collection.First().Builder.City);
                Assert.Null(collection.First().Tenant);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderSelectNamefilterEqAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($select=Name)&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Single(collection);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Null(collection.First().Builder.City);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Null(collection.First().Tenant);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderSelectNameExpandCityFilterEqAndOrderBy_CityShouldBeExpanded_BuilderNameShouldBeSam_BuilderIdShouldBeZero()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($select=Name;$expand=City)&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Single(collection);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal(1, collection.First().Builder.City.Id);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Null(collection.First().Tenant);
            }
        }

        private ICollection<TModel> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null) where TModel : class where TData : class
        {
            return
            (
                DoGet
                (
                    serviceProvider.GetRequiredService<IMapper>(),
                    serviceProvider.GetRequiredService<MyDbContext>()
                )
            ).ToList();

            IQueryable<TModel> DoGet(IMapper mapper, MyDbContext context)
            {
                return context.Set<TData>().GetQuery
                (
                    mapper,
                    options ?? GetODataQueryOptions<TModel>(query),
                    new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False } }
                );
            }
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
        {
            return
            (
                await DoGet
                (
                    serviceProvider.GetRequiredService<IMapper>()
                )
            ).ToList();

            async Task<IQueryable<TModel>> DoGet(IMapper mapper)
            {
                return await dataQueryable.GetQueryAsync
                (
                    mapper,
                    options ?? GetODataQueryOptions<TModel>(query, customNamespace),
                    querySettings
                );
            }
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
        {
            return await GetAsync
            (
                query,
                serviceProvider.GetRequiredService<MyDbContext>().Set<TData>(),
                options,
                querySettings,
                customNamespace
            );
        }

        private Task<ICollection<TModel>> GetUsingCustomNameSpace<TModel, TData>(string query,
            ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
            => GetAsync<TModel, TData>(query, options, querySettings, "com.FooBar");

        private ODataQueryOptions<TModel> GetODataQueryOptions<TModel>(string query, string customNamespace = null) where TModel : class
        {
            return ODataHelpers.GetODataQueryOptions<TModel>
            (
                query,
                serviceProvider,
                customNamespace
            );
        }
    }

    public class GetQuerySelectTestsFixture
    {
        public GetQuerySelectTestsFixture()
        {
            IServiceCollection services = new ServiceCollection();
            IMvcCoreBuilder builder = new TestMvcCoreBuilder
            {
                Services = services
            };

            builder.AddOData();
            services.AddDbContext<MyDbContext>
                (
                    options => options.UseSqlServer
                    (
                        @"Server=(localdb)\mssqllocaldb;Database=GetQuerySelectTestsatabase;ConnectRetryCount=0",
                        options => options.EnableRetryOnFailure()
                    ),
                    ServiceLifetime.Transient
                )
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddRouting()
                .AddLogging();

            ServiceProvider = services.BuildServiceProvider();

            MyDbContext context = ServiceProvider.GetRequiredService<MyDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            DatabaseInitializer.SeedDatabase(context);
        }

        internal IServiceProvider ServiceProvider;
    }
}
