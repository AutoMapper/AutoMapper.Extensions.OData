using AutoMapper.AspNet.OData;
using AutoMapper.OData.EFCore.Tests.Data;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.OData.EFCore.Tests.Binders;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Xunit;
using AutoMapper.OData.EFCore.Tests.Model;

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
        public async void OpsTenantCreatedOnFilterServerUTCTimeZone()
        {
            var querySettings = new QuerySettings
            {
                ODataSettings = new ODataSettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False,
                    TimeZone = TimeZoneInfo.Utc
                }
            };

            Test(Get<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00Z", querySettings: querySettings));

            Test(Get<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-11T19:00:00-05:00", querySettings: querySettings));

            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00Z", querySettings: querySettings));

            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-11T19:00:00-05:00", querySettings: querySettings));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async void OpsTenantCreatedOnFilterServerESTTimeZone()
        {
            var querySettings = new QuerySettings
            {
                ODataSettings = new ODataSettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False,
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                }
            };

            Test(Get<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-12T05:00:00Z", querySettings: querySettings));

            Test(Get<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00-05:00", querySettings: querySettings));

            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-12T05:00:00Z", querySettings: querySettings));

            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00-05:00", querySettings: querySettings));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy()
        {
            Test(Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc"));

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
            Test(Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc"));

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
            Test(Get<OpsTenant, TMandator>("/opstenant?$filter=Name eq 'One'"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$filter=Name eq 'One'"));

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
            Test(Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$orderby=Name desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings&$orderby=Name desc"));

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
            Test(Get<OpsTenant, TMandator>("/opstenant?$orderby=Name desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$orderby=Name desc"));

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
            Test(Get<OpsTenant, TMandator>("/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc"));

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
            Test(Get<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc"));

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
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Name eq 'One L1'"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Name eq 'One L1'"));

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
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Builder/Name eq 'Sam'&$orderby=Name asc"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Builder/Name eq 'Sam'&$orderby=Name asc"));

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
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Name ne 'One L2'&$orderby=Name desc"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Name ne 'One L2'&$orderby=Name desc"));

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
                serviceProvider
            );
            Test(Get<CoreBuilding, TBuilding>(query, options));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options));

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
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByNameThenByIdentity()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderName()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Builder/Name"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Builder/Name"));

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
                serviceProvider
            );
            Test(Get<CoreBuilding, TBuilding>(query, options));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options));

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
                serviceProvider
            );

            Test(Get<CoreBuilding, TBuilding>(query, options));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Null(options.Request.ODataFeature().TotalCount);
                Assert.Equal(1, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingSelectNameWithoutOrderWithoutTop()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$select=Name"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$select=Name"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Fact]
        public async void BuildingWithoutTopAndPageSize()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Fact]
        public async void BuildingWithTopOnly()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=3"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=3"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
            }
        }

        [Fact]
        public async void BuildingWithPageSizeOnly()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding", querySettings: new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 2 } }));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding", querySettings: new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 2 } }));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async void BuildingWithTopAndSmallerPageSize()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=3", querySettings: new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 2 } }));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=3", querySettings: new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 2 } }));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async void BuildingWithTopAndLargerPageSize()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$top=3", null, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 4 } }));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$top=3", null, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 4 } }));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
            }
        }

        [Fact]
        public async void BuildingWithTopAndSmallerPageSizeNextLink()
        {
            int pageSize = 2;
            string query = "/corebuilding?$top=3";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );

            Test(Get<CoreBuilding, TBuilding>(query, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = pageSize } }));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = pageSize } }));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);

                Uri nextPageLink = options.Request.ODataFeature().NextLink;
                Assert.NotNull(nextPageLink);
                Assert.Equal("localhost:16324/corebuilding?$top=1&$skip=2", nextPageLink.AbsoluteUri);
                Assert.Contains("$top=1", nextPageLink.Query);
                Assert.Contains("$skip=2", nextPageLink.Query);
            }
        }

        [Fact]
        public async void BuildingWithTopAndLargerPageSizeNextLink()
        {
            int pageSize = 4;
            string query = "/corebuilding?$top=3";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );

            Test(Get<CoreBuilding, TBuilding>(query, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = pageSize } }));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = pageSize } }));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
                Assert.Null(options.Request.ODataFeature().NextLink);
            }
        }

        [Fact]
        public async void OpsTenantOrderByCountOfReference()
        {
            Test(Get<OpsTenant, TMandator>("/opstenant?$expand=Buildings&$orderby=Buildings/$count desc"));
            Test(await GetAsync<OpsTenant, TMandator>("/opstenant?$expand=Buildings&$orderby=Buildings/$count desc"));

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
        [Fact]
        public async void CoreBuildingOrderByCountOfChildReferenceOfReference()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$expand=Tenant($expand=Buildings)&$orderby=Tenant/Buildings/$count desc"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$expand=Tenant($expand=Buildings)&$orderby=Tenant/Buildings/$count desc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Tenant.Buildings);
                Assert.Equal(3, collection.First().Tenant.Buildings.Count);
                Assert.Equal(2, collection.Last().Tenant.Buildings.Count);
            }
        }

        [Fact]
        public async void CoreBuildingOrderByPropertyOfChildReferenceOfReference()
        {
            Test(Get<CoreBuilding, TBuilding>("/corebuilding?$expand=Builder($expand=City)&$orderby=Builder/City/Name desc"));
            Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$expand=Builder($expand=City)&$orderby=Builder/City/Name desc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Leeds", collection.Last().Builder.City.Name);
            }
        }

        [Fact]
        public async Task CancellationThrowsException()
        {
            var cancelledToken = new CancellationTokenSource(TimeSpan.Zero).Token;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetAsync<CoreBuilding, TBuilding>("/corebuilding", querySettings: new QuerySettings { AsyncSettings = new AsyncSettings { CancellationToken = cancelledToken } }));
        }

        private ICollection<TModel> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
        {
            return DoGet
            (
                serviceProvider.GetRequiredService<IMapper>(),
                serviceProvider.GetRequiredService<MyDbContext>()
            );

            ICollection<TModel> DoGet(IMapper mapper, MyDbContext context)
            {
                return context.Set<TData>().Get
                (
                    mapper,
                    options ?? GetODataQueryOptions<TModel>(query),
                    querySettings
                );
            }
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
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
                    options ?? GetODataQueryOptions<TModel>(query),
                    querySettings
                );
            }
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

    public static class ODataHelpers
    {
        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString, IServiceProvider serviceProvider) where T : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<T>(typeof(T).Name);
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            ODataPath path = new ODataPath(new Microsoft.OData.UriParser.EntitySetSegment(entitySet));

            var request = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            }.Request;
            
            var oDataOptions = new ODataOptions().AddRouteComponents("key", model,
                x => x.AddSingleton<ISearchBinder, OpsTenantSearchBinder>());
            var (_, routeProvider) = oDataOptions.RouteComponents["key"];
            
            request.ODataFeature().Services = routeProvider;
            var oDataQueryOptions = new ODataQueryOptions<T>
            (
                new ODataQueryContext(model, typeof(T), path),
                BuildRequest(request, new Uri(BASEADDRESS + queryString))
            );
            return oDataQueryOptions;
            
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

    internal class TestMvcCoreBuilder : IMvcCoreBuilder
    {
        public ApplicationPartManager PartManager { get; set; }
        public IServiceCollection Services { get; set; }
    }
}
