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
    public class GetApplyTests
    {
        public GetApplyTests()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        private void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            IMvcCoreBuilder builder = new TestMvcCoreBuilder
            {
                Services = services
            };

            builder.AddOData();
            services.AddDbContext<MyDbContext>
                (
                    options =>
                    {
                        options.UseInMemoryDatabase("MyDbContext");
                        options.UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider());
                    },
                    ServiceLifetime.Transient
                )
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddRouting()
                .AddLogging();

            serviceProvider = services.BuildServiceProvider();

            MyDbContext context = serviceProvider.GetRequiredService<MyDbContext>();
            context.Database.EnsureCreated();
            DatabaseInitializer.SeedDatabase(context);
        }

        [Fact]
        public async void OpsTenantGroupbyNameCreatedDate()
        {
            Test(Get<OpsTenant, TMandator>("/opstenant?$apply=groupby((Name, CreatedDate))"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$apply=groupby((Name, CreatedDate))"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal("One", collection.First().Name);
                Assert.Equal("Two", collection.Last().Name);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal(3, collection.Last().Buildings.Count);
            }
        }

        [Fact]
        public async void OpsTenantFilterName()
        {
            Test(Get<OpsTenant, TMandator>("/opstenant?$apply=filter(Name eq 'One')"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$apply=filter(Name eq 'One')"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingGroupbyTenantAggregateFloorTotal()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$apply=groupby((Builder), aggregate(FloorAmount with sum as FloorTotal))&$orderby=FloorTotal"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$apply=groupby((Builder), aggregate(FloorAmount with sum as FloorTotal))&$orderby=FloorTotal"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                // CoreBuilding does not have .FloorTotal
                // Assert.Equal(9, collection.First().FloorTotal); 
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

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
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
                    options ?? GetODataQueryOptions<TModel>(query),
                    querySettings
                );
            }
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
        {
            return await GetAsync
            (
                query,
                serviceProvider.GetRequiredService<MyDbContext>().Set<TData>(),
                options,
                querySettings
            );
        }

        private ODataQueryOptions _oDataQueryOptions;
        private ODataQueryOptions<TModel> GetODataQueryOptions<TModel>(string query) where TModel : class
        {
            if (_oDataQueryOptions == null)
            {
                _oDataQueryOptions = ODataHelpers.GetODataQueryOptions<TModel>
                (
                    query,
                    serviceProvider
                );
            }

            return (ODataQueryOptions<TModel>)_oDataQueryOptions;
        }
    }
}
