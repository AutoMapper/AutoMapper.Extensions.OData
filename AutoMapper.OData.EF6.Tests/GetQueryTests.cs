using AutoMapper.AspNet.OData;
using AutoMapper.OData.EF6.Tests.Data;
using AutoMapper.OData.EF6.Tests.Model;
using DAL.EF6;
using Domain.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.OData.EF6.Tests
{
    public class GetQueryTests
    {
        public GetQueryTests()
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
            services.AddTransient<TestDbContext>(_ => new TestDbContext())
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddRouting()
                .AddLogging();

            serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void IsConfigurationValid()
        {
            serviceProvider.GetRequiredService<IConfigurationProvider>().AssertConfigurationIsValid();
        }

        [Fact]
        public async void OpsTenantSearch()
        {
            string query = "/opstenant?$search=One";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }
        
        [Fact]
        public async void OpsTenantSearchAndFilter()
        {
            string query = "/opstenant?$search=One&$filter=Name eq 'Two'";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(0, collection.Count);
            }
        }
        
        [Fact]
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsNestedFilterEqUsingThisParameterWithNoMatches()
        {
            const string query = "/opstenant?$expand=Buildings($filter=$this/Parameter eq 'FakeParameter')";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Empty(collection.First().Buildings);
                Assert.Empty(collection.Last().Buildings);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsNestedFilterEqUsingThisParameterWithMatches()
        {
            const string query = "/opstenant?$expand=Buildings($filter=$this/Name eq 'Two L1')&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Single(collection.First().Buildings);
                Assert.Equal("Two L1", collection.First().Buildings.First().Name);
                Assert.Empty(collection.Last().Buildings);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

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
            string query = "/opstenant?$filter=Name eq 'One'";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantFilterGtDateNoExpand()
        {
            string query = "/opstenant?$filter=CreatedDate gt 2012-11-11T12:00:00.00Z";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantFilterLtDateNoExpand()
        {
            string query = "/opstenant?$filter=CreatedDate lt 2012-11-11T12:00:00.00Z";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(0, collection.Count);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsNoFilterAndOrderBy()
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
        public async void OpsTenantNoExpandNoFilterAndOrderBy()
        {
            string query = "/opstenant?$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantNoExpandFilterEqAndOrderBy()
        {
            string query = "/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsSelectNameAndBuilderExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$select=Name&$expand=Buildings($select=Name,Builder;$expand=Builder($select=Name,City;$expand=City))&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.NotEqual(default, collection.First().Buildings.First().Builder.City.Name);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async void OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

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
        public async void BuildingExpandBuilderTenantFilterEqAndOrderByWithParameter()
        {
            string buildingParameterValue = Guid.NewGuid().ToString();
            int builderParameterValue = new Random().Next();
            var parameters = new
            {
                buildingParameter = buildingParameterValue,
                builderParameter = builderParameterValue
            };

            string query = "/corebuilding?$top=1&$expand=Builder&$filter=Name eq 'One L1'";
            var projectionSettings = new ProjectionSettings { Parameters = parameters };
            var odataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False };
            Test
            (
                Get<CoreBuilding, TBuilding>
                (
                    query,
                    null,
                    new QuerySettings
                    {
                        ProjectionSettings = projectionSettings,
                        ODataSettings = odataSettings
                    }
                )
            );
            Test
            (
                await GetAsync<CoreBuilding, TBuilding>
                (
                    query,
                    null,
                    new QuerySettings
                    {
                        ProjectionSettings = projectionSettings,
                        ODataSettings = odataSettings
                    }
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CoreBuilding, TBuilding>
                (
                    query,
                    null,
                    new QuerySettings
                    {
                        ProjectionSettings = projectionSettings,
                        ODataSettings = odataSettings
                    }
                )
            );

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Equal(buildingParameterValue, collection.First().Parameter);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(builderParameterValue, collection.First().Builder.Parameter);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderTenantFilterEqAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async void BuildingExpandBuilderSelectNameExpandTenantFilterEqAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($select=Name),Tenant&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

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
        public async void BuildingExpandBuilderTenantExpandCityFilterOnPropertyAndOrderBy()
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
        public async void BuildingExpandBuilderTenantExpandCityOrderByName()
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
        public async void BuildingExpandBuilderTenantExpandCityOrderByNameThenByIdentity()
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
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderName()
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
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, options));

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
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query, options));

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
        public async void BuildingWithoutTopAndPageSize()
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
        public async void BuildingWithTopOnly()
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
        public async void BuildingWithPageSizeOnly()
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
        public async void BuildingWithTopAndSmallerPageSize()
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
        public async void BuildingWithTopAndLargerPageSize()
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
        public async void BuildingWithTopAndSmallerPageSizeNextLink()
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
        public async void BuildingWithTopAndLargerPageSizeNextLink()
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
        public async void OpsTenantOrderByCountOfReference()
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
        public async void OpsTenantOrderByFilteredCount()
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

        //System.NotSupportedException
        /*
         *  StackTrace:
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.ValidateInitializerMetadata(InitializerMetadata metadata)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.MemberInitTranslator.TypedTranslate(ExpressionConverter parent, MemberInitExpression linq)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.TypedTranslator`1.Translate(ExpressionConverter parent, Expression linq)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.TranslateExpression(Expression linq)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.TranslateLambda(LambdaExpression lambda, DbExpression input)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.TranslateLambda(LambdaExpression lambda, DbExpression input, DbExpressionBinding& binding)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.MethodCallTranslator.OneLambdaTranslator.Translate(ExpressionConverter parent, MethodCallExpression call, DbExpression& source, DbExpressionBinding& sourceBinding, DbExpression& lambda)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.MethodCallTranslator.SelectTranslator.Translate(ExpressionConverter parent, MethodCallExpression call)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.MethodCallTranslator.SequenceMethodTranslator.Translate(ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.MethodCallTranslator.TypedTranslate(ExpressionConverter parent, MethodCallExpression linq)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.TypedTranslator`1.Translate(ExpressionConverter parent, Expression linq)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.TranslateExpression(Expression linq)
   at System.Data.Entity.Core.Objects.ELinq.ExpressionConverter.Convert()
   at System.Data.Entity.Core.Objects.ELinq.ELinqQueryState.GetExecutionPlan(Nullable`1 forMergeOption)
   at System.Data.Entity.Core.Objects.ObjectQuery`1.<>c__DisplayClass41_0.<GetResults>b__1()
   at System.Data.Entity.Core.Objects.ObjectContext.ExecuteInTransaction[T](Func`1 func, IDbExecutionStrategy executionStrategy, Boolean startLocalTransaction, Boolean releaseConnectionOnSuccess)
   at System.Data.Entity.Core.Objects.ObjectQuery`1.<>c__DisplayClass41_0.<GetResults>b__0()
   at System.Data.Entity.SqlServer.DefaultSqlExecutionStrategy.Execute[TResult](Func`1 operation)
   at System.Data.Entity.Core.Objects.ObjectQuery`1.GetResults(Nullable`1 forMergeOption)
   at System.Data.Entity.Core.Objects.ObjectQuery`1.<System.Collections.Generic.IEnumerable<T>.GetEnumerator>b__31_0()
   at System.Data.Entity.Internal.LazyEnumerator`1.MoveNext()
   at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at AutoMapper.OData.EF6.Tests.GetQueryTests.GetAsync[TModel,TData](String query, IQueryable`1 dataQueryable, ODataQueryOptions`1 options, QuerySettings querySettings) in C:\.github\AutoMapper\AutoMapper.Extensions.OData\AutoMapper.OData.EF6.Tests\GetQueryTests.cs:line 861
   at AutoMapper.OData.EF6.Tests.GetQueryTests.GetAsync[TModel,TData](String query, ODataQueryOptions`1 options, QuerySettings querySettings) in C:\.github\AutoMapper\AutoMapper.Extensions.OData\AutoMapper.OData.EF6.Tests\GetQueryTests.cs:line 882
   at AutoMapper.OData.EF6.Tests.GetQueryTests.CoreBuildingOrderByCountOfChildReferenceOfReference() in C:\.github\AutoMapper\AutoMapper.Extensions.OData\AutoMapper.OData.EF6.Tests\GetQueryTests.cs:line 558
   at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__139_0(Object state)
Result Message:	System.NotSupportedException : The type 'Domain.OData.CoreBuilding' appears in two structurally incompatible initializations within a single LINQ to Entities query. A type can be initialized in two places in the same query, but only if the same properties are set in both places and those properties are set in the same order.
         */
        //[Fact]
        //public async void CoreBuildingOrderByCountOfChildReferenceOfReference()
        //{
        //    Test(await GetAsync<CoreBuilding, TBuilding>("/corebuilding?$expand=Tenant($expand=Buildings)&$orderby=Tenant/Buildings/$count desc"));
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

        private IQueryable<Category> GetCategories()
         => new Category[]
            {
                new Category
                {
                    CategoryID = 1,
                    CategoryName = "CategoryOne",
                    Products = new Product[]
                    {
                        new Product
                        {
                            ProductID = 1,
                            ProductName = "ProductOne",
                            AlternateAddresses = new Address[]
                            {
                                new Address { AddressID = 1, City = "CityOne" },
                                new Address { AddressID = 2, City = "CityTwo"  },
                            },
                            SupplierAddress = new Address { City = "A" }
                        },
                        new Product
                        {
                            ProductID = 2,
                            ProductName = "ProductTwo",
                            AlternateAddresses = Array.Empty<Address>( ),
                            SupplierAddress = new Address { City = "B" }
                        },
                        new Product
                        {
                            ProductID = 3,
                            ProductName = "ProductThree",
                            AlternateAddresses = Array.Empty<Address>( ),
                            SupplierAddress = new Address { City = "C" }
                        },
                    },
                    CompositeKeys = new CompositeKey[]
                    {
                        new CompositeKey{ ID1 = 1, ID2 = 5 },
                        new CompositeKey{ ID1 = 1, ID2 = 4 },
                        new CompositeKey{ ID1 = 1, ID2 = 3 },
                        new CompositeKey{ ID1 = 1, ID2 = 2 },
                        new CompositeKey{ ID1 = 1, ID2 = 1 },
                    }
                },
                new Category
                {
                    CategoryID = 2,
                    CategoryName = "CategoryTwo",
                    Products =  new Product[]
                    {
                        new Product
                        {
                            ProductID = 4,
                            ProductName = "ProductFour",
                            AlternateAddresses = Array.Empty<Address>( ),
                            SupplierAddress = new Address { City = "D" }
                        },
                        new Product
                        {
                            ProductID = 5,
                            ProductName = "ProductFive",
                            AlternateAddresses = new Address[]
                            {
                                new Address { AddressID = 3, City = "CityThree" },
                                new Address { AddressID = 4, City = "CityFour"  },
                                new Address { AddressID = 5, City = "CityFive"  },
                            },
                            SupplierAddress = new Address { City = "E" }
                        }
                    },
                    CompositeKeys = new CompositeKey[]
                    {
                        new CompositeKey{ ID1 = 1, ID2 = 9 },
                        new CompositeKey{ ID1 = 2, ID2 = 5 },
                        new CompositeKey{ ID1 = 2, ID2 = 2 },
                        new CompositeKey{ ID1 = 3, ID2 = 3 },
                        new CompositeKey{ ID1 = 3, ID2 = 4 },
                    }
                }
            }.AsQueryable();

        [Fact]
        public async void FilteringOnRoot_AndChildCollection_WithMatches()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=ProductName eq 'ProductOne')&$filter=CategoryName eq 'CategoryOne'";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Single(collection.First().Products);
            }
        }

        [Fact]
        public async void FilteringOnRoot_AndChildCollection_WithNoMatches()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$orderby=ProductName desc)&$filter=CategoryName ne ''&$orderby=CategoryName asc";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(3, collection.First().Products.Count);
            }
        }

        [Fact]
        public async void FilteringOnRoot_AndChildCollection_WithNoMatches_SortRootAndChildCollection()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$orderby=ProductName desc)&$filter=CategoryName ne ''&$orderby=CategoryName asc";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal("CategoryOne", collection.First().CategoryName);
                Assert.Equal("ProductTwo", collection.First().Products.First().ProductName);
            }
        }

        [Fact]
        public async void FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithMatches()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=ProductName eq 'ProductOne';$expand=AlternateAddresses($filter=City eq 'CityOne'))&$filter=CategoryName eq 'CategoryOne'";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Single(collection.First().Products);
                Assert.Single(collection.First().Products.First().AlternateAddresses);
            }
        }

        [Fact]
        public async void FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithNoMatches()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$expand=AlternateAddresses($filter=City ne ''))&$filter=CategoryName ne ''";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(3, collection.First().Products.Count);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Count());
            }
        }

        [Fact]
        public async void FilteringOnRoot_ChildCollection_WithTopNoOrderBy_AndChildCollectionOfChildCollection_WithNoMatches()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$top=1;$expand=AlternateAddresses($filter=City ne ''))&$filter=CategoryName ne ''";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(1, collection.First().Products.Count);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Count());
            }
        }

        [Fact]
        public async void FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithNoMatches_SortRoot_AndChildCollection_AndChildCollectionOfChildCollection()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($filter=SupplierAddress/City ne '';$orderby=ProductName;$expand=AlternateAddresses($filter=City ne '';$orderby=City desc),SupplierAddress)&$filter=CategoryName ne ''&$orderby=CategoryName asc";
            Test
            (
                Get<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetAsync<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );
            Test
            (
                await GetUsingCustomNameSpace<CategoryModel, Category>
                (
                    query,
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal("CategoryOne", collection.First().CategoryName);
                Assert.Equal("ProductOne", collection.First().Products.First().ProductName);
                Assert.Equal("CityTwo", collection.First().Products.First().AlternateAddresses.First().City);
            }
        }

        [Fact]
        public async void SkipOnRootNoOrderByThenExpandAndSkipOnChildCollectionNoOrderByThenExpandSkipAndTopOnChildCollectionOfChildCollectionWithOrderBy()
        {
            const string query = "/CategoryModel?$skip=1&$expand=Products($skip=1;$expand=AlternateAddresses($skip=1;$top=3;$orderby=AddressID desc))";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().CategoryID);
                Assert.Equal(1, collection.First().Products.Count);
                Assert.Equal(5, collection.First().Products.First().ProductID);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Length);
                Assert.Equal(4, collection.First().Products.First().AlternateAddresses.First().AddressID);
                Assert.Equal(3, collection.First().Products.First().AlternateAddresses.Last().AddressID);
            }
        }

        [Fact]
        public async void SkipOnRootNoOrderByThenExpandAndSkipOnChildCollectionNoOrderByThenExpandSkipAndTopOnChildCollectionOfChildCollectionNoOrderBy()
        {
            const string query = "/CategoryModel?$skip=1&$expand=Products($skip=1;$expand=AlternateAddresses($skip=1;$top=3))";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().CategoryID);
                Assert.Equal(1, collection.First().Products.Count);
                Assert.Equal(5, collection.First().Products.First().ProductID);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Length);
                Assert.Equal(4, collection.First().Products.First().AlternateAddresses.First().AddressID);
                Assert.Equal(5, collection.First().Products.First().AlternateAddresses.Last().AddressID);
            }
        }

        [Fact]
        public async void ExpandChildCollectionWithTopAndSkipNoOrderBy()
        {
            const string query = "/CategoryModel?$expand=Products($skip=1;$top=2)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.First().Products.First().ProductID);
                Assert.Equal(3, collection.First().Products.Last().ProductID);
                Assert.Equal(1, collection.Last().Products.Count);
                Assert.Equal(5, collection.Last().Products.First().ProductID);
            }
        }

        [Fact]
        public async void ExpandChildCollectionSkipBeyondAllElementsNoOrderBy()
        {
            const string query = "/CategoryModel?$expand=Products($skip=3)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Empty(collection.First().Products);
                Assert.Empty(collection.Last().Products);
            }
        }

        [Fact]
        public async void ExpandChildCollectionWithSkipNoOrderByModelHasCompositeKey()
        {
            const string query = "/CategoryModel?$expand=CompositeKeys($skip=1)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                var first = collection.First().CompositeKeys.ToArray();
                Assert.Equal(4, first.Length);
                Assert.Equal((1, 2), (first[0].ID1, first[0].ID2));
                Assert.Equal((1, 3), (first[1].ID1, first[1].ID2));
                Assert.Equal((1, 4), (first[2].ID1, first[2].ID2));
                Assert.Equal((1, 5), (first[3].ID1, first[3].ID2));

                var second = collection.Last().CompositeKeys.ToArray();
                Assert.Equal(4, second.Length);
                Assert.Equal((2, 2), (second[0].ID1, second[0].ID2));
                Assert.Equal((2, 5), (second[1].ID1, second[1].ID2));
                Assert.Equal((3, 3), (second[2].ID1, second[2].ID2));
                Assert.Equal((3, 4), (second[3].ID1, second[3].ID2));
            }
        }

        [Fact]
        public async void ExpandChildCollectionWithSkipAndTopNoOrderByModelHasCompositeKey()
        {
            const string query = "/CategoryModel?$expand=CompositeKeys($skip=1;$top=2)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                var first = collection.First().CompositeKeys.ToArray();
                Assert.Equal(2, first.Length);
                Assert.Equal((1, 2), (first[0].ID1, first[0].ID2));
                Assert.Equal((1, 3), (first[1].ID1, first[1].ID2));

                var second = collection.Last().CompositeKeys.ToArray();
                Assert.Equal(2, second.Length);
                Assert.Equal((2, 2), (second[0].ID1, second[0].ID2));
                Assert.Equal((2, 5), (second[1].ID1, second[1].ID2));
            }
        }

        [Fact]
        public async void SkipOnRootNoOrderBy()
        {
            const string query = "/CategoryModel?$skip=1";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().CategoryID);
            }
        }

        [Fact]
        public async void SkipBeyondAllElementsOnRootNoOrderBy()
        {
            const string query = "/CategoryModel?$skip=2";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Empty(collection);
            }
        }

        [Fact]
        public async Task CancellationThrowsException()
        {
            var cancelledToken = new CancellationTokenSource(TimeSpan.Zero).Token;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetAsync<CoreBuilding, TBuilding>("/corebuilding?$count=true", querySettings: new QuerySettings { AsyncSettings = new AsyncSettings { CancellationToken = cancelledToken } }));
        }

        private ICollection<TModel> Get<TModel, TData>(string query, IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
        {
            return
            (
                DoGet
                (
                    serviceProvider.GetRequiredService<IMapper>()
                )
            ).ToList();

            IQueryable<TModel> DoGet(IMapper mapper)
            {
                return dataQueryable.GetQuery
                (
                    mapper,
                    options ?? GetODataQueryOptions<TModel>(query),
                    querySettings
                );
            }
        }

        private ICollection<TModel> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
        {
            return Get
            (
                query,
                serviceProvider.GetRequiredService<TestDbContext>().Set<TData>(),
                options,
                querySettings
            );
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
                serviceProvider.GetRequiredService<TestDbContext>().Set<TData>(),
                options,
                querySettings,
                customNamespace
            );
        }

        private Task<ICollection<TModel>> GetUsingCustomNameSpace<TModel, TData>(string query,
            IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
            => GetAsync(query, dataQueryable, options, querySettings, "com.FooBar");

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
}
