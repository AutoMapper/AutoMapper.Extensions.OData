using AutoMapper.AspNet.OData;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.OData.EFCore.Tests
{
    public class GetTests
    {
        public GetTests()
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
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

            serviceProvider = services.BuildServiceProvider();

            MyDbContext context = serviceProvider.GetRequiredService<MyDbContext>();
            context.Database.EnsureCreated();
            SeedDatabase(context);
        }

        [Fact]
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsFilterNeAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantFilterEqNoExpand()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$filter=Name eq 'One'"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(0, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsNoFilterAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantNoExpandNoFilterAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(0, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantNoExpandFilterEqAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(0, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantFilterEqAndOrderBy()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=name eq 'One L1'"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantFilterOnNestedPropertyAndOrderBy()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Builder/Name eq 'Sam'&$orderby=Name asc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityFilterOnPropertyAndOrderBy()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Name ne 'One L2'&$orderby=Name desc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(4, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("Two L3", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityFilterOnNestedNestedPropertyWithCount()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Builder/City/Name eq 'Leeds'&$count=true";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider,
                serviceProvider.GetRequiredService<IRouteBuilder>()
            );
            Test
            (
                await Get<CoreBuilding, TBuilding>
                (
                    query,
                    options
                )
            );

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, options.Request.ODataFeature().TotalCount);
                Assert.Equal(2, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByName()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByNameThenByIdentity()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderName()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Builder/Name"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Two L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1WithCount()
        {
            string query = "/corebuilding?$skip=4&$top=1&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity&$count=true";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider,
                serviceProvider.GetRequiredService<IRouteBuilder>()
            );
            Test
            (
                await Get<CoreBuilding, TBuilding>
                (
                    query,
                    options
                )
            );

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, options.Request.ODataFeature().TotalCount);
                Assert.Equal(1, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1NoCount()
        {
            string query = "/corebuilding?$skip=4&$top=1&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider,
                serviceProvider.GetRequiredService<IRouteBuilder>()
            );

            Test
            (
                await Get<CoreBuilding, TBuilding>
                (
                    query,
                    options
                )
            );

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Null(options.Request.ODataFeature().TotalCount);
                Assert.Equal(1, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingSelectName_WithoutOrder_WithoutTop()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$select=Name"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Fact]
        public async void OpsTenantOrderByCountOfReference()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$expand=Buildings&$orderby=Buildings/$count desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.NotNull(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal(2, collection.Last().Buildings.Count);
            }
        }

        //Exception: 'The Include path 'Mandator->Buildings' results in a cycle. 
        //Cycles are not allowed in no-tracking queries. Either use a tracking query or remove the cycle.'
        //[Fact]
        //public async void CoreBuildingOrderByCountOfChildReferenceOfReference()
        //{
        //    Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$expand=Tenant($expand=Buildings)&$orderby=Tenant/Buildings/$count desc"));
        //    void Test(ICollection<CoreBuilding> collection)
        //    {
        //        Assert.Equal(5, collection.Count);
        //        Assert.NotNull(collection.First().Tenant.Buildings);
        //        Assert.Equal(3, collection.First().Tenant.Buildings.Count);
        //        Assert.Equal(2, collection.Last().Tenant.Buildings.Count);
        //    }
        //}

        [Fact]
        public async void CoreBuildingOrderByPropertyOfChildReferenceOfReference()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$expand=Builder($expand=City)&$orderby=Builder/City/Name desc"));
            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Leeds", collection.Last().Builder.City.Name);
            }
        }

        private async Task<ICollection<TModel>> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null) where TModel : class where TData : class
        {
            return await DoGet
            (
                serviceProvider.GetRequiredService<IMapper>(),
                serviceProvider.GetRequiredService<MyDbContext>()
            );

            async Task<ICollection<TModel>> DoGet(IMapper mapper, MyDbContext context)
            {
                return await context.Set<TData>().GetAsync
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

        static void SeedDatabase(MyDbContext context)
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
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L2", BuilderId = builders.First(b => b.Name == "Mark").Id  },
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L3", BuilderId = builders.First(b => b.Name == "Mark").Id  }
                }
            });
            context.SaveChanges();
        }
    }

    public static class ODataHelpers
    {
        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString, IServiceProvider serviceProvider, IRouteBuilder routeBuilder) where T : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(serviceProvider);

            builder.EntitySet<T>(typeof(T).Name);
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            ODataPath path = new ODataPath(new Microsoft.OData.UriParser.EntitySetSegment(entitySet));

            routeBuilder.EnableDependencyInjection();

            return new ODataQueryOptions<T>
            (
                new ODataQueryContext(model, typeof(T), path),
                BuildRequest
                (
                    new DefaultHttpContext()
                    {
                        RequestServices = serviceProvider
                    }.Request,
                    new Uri(BASEADDRESS + queryString)
                )
            );

            static HttpRequest BuildRequest(HttpRequest request, Uri uri)
            {
                request.Method = "GET";
                request.Host = new HostString(uri.Host, uri.Port);
                request.Path = uri.LocalPath;
                request.QueryString = new QueryString(uri.Query);

                return request;
            }

        }

        static readonly string BASEADDRESS = "http://localhost:16324";
    }
}
