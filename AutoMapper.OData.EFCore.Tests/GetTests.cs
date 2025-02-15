using AutoMapper.AspNet.OData;
using AutoMapper.OData.EFCore.Tests.Binders;
using AutoMapper.OData.EFCore.Tests.Data;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
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
using Xunit;

namespace AutoMapper.OData.EFCore.Tests
{
    public class GetTests : IClassFixture<GetTestsFixture>
    {
        private readonly GetTestsFixture _fixture;

        public GetTests(GetTestsFixture fixture)
        {
            _fixture = fixture;
            serviceProvider = _fixture.ServiceProvider;
        }

        #region Fields
        private readonly IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task OpsTenantCreatedOnFilterServerUTCTimeZone()
        {
            var querySettings = new QuerySettings
            {
                ODataSettings = new ODataSettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False,
                    TimeZone = TimeZoneInfo.Utc
                }
            };

            string query = "/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00Z";
            Test(Get<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, querySettings: querySettings));

            query = "/opstenant?$filter=CreatedDate eq 2012-12-11T19:00:00-05:00";
            Test(Get<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, querySettings: querySettings));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async Task OpsTenantCreatedOnFilterServerESTTimeZone()
        {
            var querySettings = new QuerySettings
            {
                ODataSettings = new ODataSettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False,
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                }
            };

            string query = "/opstenant?$filter=CreatedDate eq 2012-12-12T05:00:00Z";
            Test(Get<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, querySettings: querySettings));

            query = "/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00-05:00";
            Test(Get<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, querySettings: querySettings));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsFilterEqAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantFilterEqNoExpand()
        {
            string query = "/opstenant?$filter=Name eq 'One'";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Empty(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsNoFilterAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantNoExpandNoFilterAndOrderBy()
        {
            string query = "/opstenant?$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Empty(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantNoExpandFilterEqAndOrderBy()
        {
            string query = "/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Empty(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantFilterEqAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Single(collection);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantFilterOnNestedPropertyAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Builder/Name eq 'Sam'&$orderby=Name asc";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityFilterOnPropertyAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Name ne 'One L2'&$orderby=Name desc";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(4, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("Two L3", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityFilterOnNestedNestedPropertyWithCount()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Builder/City/Name eq 'Leeds'&$count=true";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );
            Test(Get<CoreBuilding, TBuilding>(query, options));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options));
            Test
            (
                await GetUsingCustomNameSpace<CoreBuilding, TBuilding>
                (
                    query,
                    ODataHelpers.GetODataQueryOptions<CoreBuilding>
                    (
                        query,
                        serviceProvider,
                        "com.FooBar"
                    )
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
        public async Task BuildingExpandBuilderTenantExpandCityOrderByName()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityOrderByNameThenByIdentity()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityOrderByBuilderName()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Builder/Name";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Two L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1WithCount()
        {
            string query = "/corebuilding?$skip=4&$top=1&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity&$count=true";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );
            Test(Get<CoreBuilding, TBuilding>(query, options));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, options));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, options.Request.ODataFeature().TotalCount);
                Assert.Single(collection);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1NoCount()
        {
            string query = "/corebuilding?$skip=4&$top=1&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity";
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );

            Test(Get<CoreBuilding, TBuilding>(query, options));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, options));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Null(options.Request.ODataFeature().TotalCount);
                Assert.Single(collection);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingSelectNameWithoutOrderWithoutTop()
        {
            string query = "/corebuilding?$select=Name";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Fact]
        public async Task BuildingWithoutTopAndPageSize()
        {
            string query = "/corebuilding";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Fact]
        public async Task BuildingWithTopOnly()
        {
            string query = "/corebuilding?$top=3";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
            }
        }

        [Fact]
        public async Task BuildingWithPageSizeOnly()
        {
            string query = "/corebuilding";
            var querySettings = new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 2 } };
            Test(Get<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, querySettings: querySettings));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async Task BuildingWithTopAndSmallerPageSize()
        {
            string query = "/corebuilding?$top=3";
            var querySettings = new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 2 } };
            Test(Get<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, querySettings: querySettings));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
            }
        }

        [Fact]
        public async Task BuildingWithTopAndLargerPageSize()
        {
            string query = "/corebuilding?$top=3";
            var querySettings = new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = 4 } };
            Test(Get<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, querySettings: querySettings));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
            }
        }

        [Fact]
        public async Task BuildingWithTopAndSmallerPageSizeNextLink()
        {
            int pageSize = 2;
            string query = "/corebuilding?$top=3";
            var querySettings = new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = pageSize } };
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );

            Test(Get<CoreBuilding, TBuilding>(query, options, querySettings));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options, querySettings));
            Test
            (
                await GetUsingCustomNameSpace<CoreBuilding, TBuilding>
                (
                    query,
                    ODataHelpers.GetODataQueryOptions<CoreBuilding>
                    (
                        query,
                        serviceProvider,
                        "com.FooBar"
                    ),
                    querySettings
                )
            );

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
        public async Task BuildingWithTopAndLargerPageSizeNextLink()
        {
            int pageSize = 4;
            string query = "/corebuilding?$top=3";
            var querySettings = new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False, PageSize = pageSize } };
            ODataQueryOptions<CoreBuilding> options = ODataHelpers.GetODataQueryOptions<CoreBuilding>
            (
                query,
                serviceProvider
            );

            Test(Get<CoreBuilding, TBuilding>(query, options, querySettings));
            Test(await GetAsync<CoreBuilding, TBuilding>(query, options, querySettings));
            Test
            (
                await GetUsingCustomNameSpace<CoreBuilding, TBuilding>
                (
                    query,
                    ODataHelpers.GetODataQueryOptions<CoreBuilding>
                    (
                        query,
                        serviceProvider,
                        "com.FooBar"
                    ),
                    querySettings
                )
            );

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(3, collection.Count);
                Assert.Null(options.Request.ODataFeature().NextLink);
            }
        }

        [Fact]
        public async Task OpsTenantOrderByCountOfReference()
        {
            string query = "/opstenant?$expand=Buildings&$orderby=Buildings/$count desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.NotNull(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal(2, collection.Last().Buildings.Count);
            }
        }
        
        [Fact]
        public async Task OpsTenantOrderByFilteredCount()
        {
            string query = "/opstenant?$expand=Buildings&$orderby=Buildings/$count($filter=Name eq 'One L1') desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.NotNull(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal(3, collection.Last().Buildings.Count);
            }
        }

        //Exception: 'The Include path 'Mandator->Buildings' results in a cycle.
        //Cycles are not allowed in no-tracking queries. Either use a tracking query or remove the cycle.'
        [Fact]
        public async Task CoreBuildingOrderByCountOfChildReferenceOfReference()
        {
            string query = "/corebuilding?$expand=Tenant($expand=Buildings)&$orderby=Tenant/Buildings/$count desc";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Tenant.Buildings);
                Assert.Equal(3, collection.First().Tenant.Buildings.Count);
                Assert.Equal(2, collection.Last().Tenant.Buildings.Count);
            }
        }

        [Fact]
        public async Task CoreBuildingOrderByPropertyOfChildReferenceOfReference()
        {
            string query = "/corebuilding?$expand=Builder($expand=City)&$orderby=Builder/City/Name desc";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

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

        private ICollection<TModel> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
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
                    options ?? GetODataQueryOptions<TModel>(query, customNamespace),
                    querySettings
                );
            }
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
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
                    options ?? GetODataQueryOptions<TModel>(query, customNamespace),
                    querySettings
                );
            }
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

    public static class ODataHelpers
    {
        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString, IServiceProvider serviceProvider, string customNamespace = null, bool enableLowerCamelCase = false) where T : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            if (customNamespace != null)
                builder.Namespace = customNamespace;

            if (enableLowerCamelCase)
                builder.EnableLowerCamelCase();

            builder.EntitySet<T>(typeof(T).Name);
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            var oDataQueryOptions = new ODataQueryOptions<T>
            (
                new ODataQueryContext(model, typeof(T), path),
                BuildRequest(serviceProvider, model, new Uri(BASEADDRESS + queryString))
            );

            return oDataQueryOptions;
        }

        public static ODataQueryOptions<Model.CategoryModel> GetODataQueryOptionsWithDuplicateEntityName(string queryString, IServiceProvider serviceProvider, string customNamespace = null)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            if (customNamespace != null)
                builder.Namespace = customNamespace;

            builder.EnableLowerCamelCase();

            builder.EntitySet<X.CategoryModel>(typeof(X.CategoryModel).Name + "X");
            builder.EntitySet<Model.CategoryModel>(nameof(Model.CategoryModel));
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(Model.CategoryModel).Name);
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            var oDataQueryOptions = new ODataQueryOptions<Model.CategoryModel>
            (
                new ODataQueryContext(model, typeof(Model.CategoryModel), path),
                BuildRequest(serviceProvider, model, new Uri(BASEADDRESS + queryString))
            );

            return oDataQueryOptions;
        }

        static HttpRequest BuildRequest(IServiceProvider serviceProvider, IEdmModel model, Uri uri)
        {
            var request = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            }.Request;

            var oDataOptions = new ODataOptions().AddRouteComponents("key", model,
                x => x.AddSingleton<ISearchBinder, OpsTenantSearchBinder>());
            var (_, routeProvider) = oDataOptions.RouteComponents["key"];

            request.ODataFeature().Services = routeProvider;

            request.Method = "GET";
            request.Host = new HostString(uri.Host, uri.Port);
            request.Path = uri.LocalPath;
            request.QueryString = new QueryString(uri.Query);

            return request;
        }

        static readonly string BASEADDRESS = "http://localhost:16324";
    }

    internal class TestMvcCoreBuilder : IMvcCoreBuilder
    {
        public ApplicationPartManager PartManager { get; set; }
        public IServiceCollection Services { get; set; }
    }

    public class GetTestsFixture
    {
        public GetTestsFixture()
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
                        @"Server=(localdb)\mssqllocaldb;Database=GetTestsDatabase;ConnectRetryCount=0",
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
