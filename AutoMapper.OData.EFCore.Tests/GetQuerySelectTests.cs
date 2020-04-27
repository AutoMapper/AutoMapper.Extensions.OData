using AutoMapper.AspNet.OData;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.OData.EFCore.Tests
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
            services.AddDbContext<MyDbContext>
                (
                    options =>
                    {
                        options.UseInMemoryDatabase("MyDbContext");
                        options.UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider());
                    },
                    ServiceLifetime.Transient
                )
                .AddSingleton<AutoMapper.IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new Microsoft.AspNetCore.Builder.Internal.ApplicationBuilder(sp))
                .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

            serviceProvider = services.BuildServiceProvider();

            MyDbContext context = serviceProvider.GetRequiredService<MyDbContext>();
            context.Database.EnsureCreated();
            Seed_Database(context);
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
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$select=Buildings&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.True(collection.Count == 1);
                Assert.True(collection.First().Buildings.Count == 2);
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
                    serviceProvider.GetRequiredService<MyDbContext>()
                )
            ).ToList();

            async Task<IQueryable<TModel>> DoGet(IMapper mapper, MyDbContext context)
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
                    HandleNullPropagationOption.False
                );
            }
        }

        static void Seed_Database(MyDbContext context)
        {
            context.City.Add(new TCity { Name = "London" });
            context.City.Add(new TCity { Name = "Leeds" });
            context.SaveChanges();

            List<TCity> cities = context.City.ToList();
            context.Builder.Add(new TBuilder { Name = "Sam", CityId = cities.First(b => b.Name == "London").Id });
            context.Builder.Add(new TBuilder { Name = "John", CityId = cities.First(b => b.Name == "London").Id });
            context.Builder.Add(new TBuilder { Name = "Mark", CityId = cities.First(b => b.Name == "Leeds").Id });
            context.SaveChanges();

            List<TBuilder> builders = context.Builder.ToList();
            context.MandatorSet.Add(new TMandator
            {
                Identity = Guid.NewGuid(),
                Name = "One",
                Buildings = new List<TBuilding>
                {
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "One L1", BuilderId = builders.First(b => b.Name == "Sam").Id },
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "One L2", BuilderId = builders.First(b => b.Name == "Sam").Id  }
                }
            });
            context.MandatorSet.Add(new TMandator
            {
                Identity = Guid.NewGuid(),
                Name = "Two",
                Buildings = new List<TBuilding>
                {
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L1", BuilderId = builders.First(b => b.Name == "John").Id  },
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L2", BuilderId = builders.First(b => b.Name == "Mark").Id  }
                }
            });
            context.SaveChanges();
        }
    }
}
