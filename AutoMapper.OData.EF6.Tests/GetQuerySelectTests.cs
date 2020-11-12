using AutoMapper.AspNet.OData;
using AutoMapper.OData.EF6.Tests.Data;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.OData.EF6.Tests
{
    public class GetQuerySelectTests
    {
        public GetQuerySelectTests()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        private void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOData();
            services.AddTransient<TestDbContext>(_ => new TestDbContext())
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

            serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async void OpsTenantSelectName()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$select=Name&$expand=Buildings&$orderby=Name"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
                Assert.Equal(default, collection.First().Identity);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy_FirstBuildingHasValues()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$select=Buildings&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Name);
                Assert.NotEqual(default, collection.First().Buildings.First().Identity);
                Assert.Equal(default, collection.First().Identity);
                Assert.Null(collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingSelectNameExpandBuilder_Builder_ShouldBeNull()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$select=Name&$expand=Builder($select=Name)&$filter=name eq 'One L1'"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Null(collection.First().Builder);
                Assert.Null(collection.First().Tenant);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderSelectNamefilterEqAndOrderBy()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($select=Name)&$filter=name eq 'One L1'"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Null(collection.First().Builder.City);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Null(collection.First().Tenant);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderSelectNameExpandCityFilterEqAndOrderBy_CityShouldBeNull_BuilderNameShouldeSam_BuilderIdShouldBeZero()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($select=Name;$expand=City)&$filter=name eq 'One L1'"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Null(collection.First().Builder.City);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Null(collection.First().Tenant);
            }
        }

        private async Task<ICollection<TModel>> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null) where TModel : class where TData : class
        {
            return
            (
                await DoGet
                (
                    serviceProvider.GetRequiredService<IMapper>(),
                    serviceProvider.GetRequiredService<TestDbContext>()
                )
            ).ToList();

            async Task<IQueryable<TModel>> DoGet(IMapper mapper, TestDbContext context)
            {
                return await context.Set<TData>().GetQueryAsync
                (
                    mapper,
                    options ?? ODataHelpers.GetODataQueryOptions<TModel>
                    (
                        query,
                        serviceProvider,
                        serviceProvider.GetRequiredService<IRouteBuilder>()
                    ),
                    new QuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }
                );
            }
        }
    }
}
