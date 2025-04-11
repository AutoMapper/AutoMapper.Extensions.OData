using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.AspNet.OData;
using AutoMapper.OData.EFCore.Tests.Data;
using AutoMapper.OData.EFCore.Tests.Model;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoMapper.OData.EFCore.Tests
{
    public class GetQueryTests : IClassFixture<GetQueryTestsFixture>
    {
        private readonly GetQueryTestsFixture _fixture;

        public GetQueryTests(GetQueryTestsFixture fixture)
        {
            _fixture = fixture;
            serviceProvider = _fixture.ServiceProvider;
        }

        #region Fields
        private readonly IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public void IsConfigurationValid()
        {
            serviceProvider.GetRequiredService<IConfigurationProvider>().AssertConfigurationIsValid();
        }
        
        [Fact]
        public async Task OpsTenantSearch()
        {
            string query = "/opstenant?$search=One";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal("One", collection.First().Name);
            }
        }
        
        [Fact]
        public async Task OpsTenantSearchAndFilter()
        {
            string query = "/opstenant?$search=One&$filter=Name eq 'Two'";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Empty(collection);
            }
        }

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
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query, querySettings: querySettings));
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
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, querySettings: querySettings));

            query = "/opstenant?$filter=CreatedDate eq 2012-12-12T00:00:00-05:00";
            Test(Get<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, querySettings: querySettings));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query, querySettings: querySettings));
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
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsNestedFilterEqUsingThisParameterWithNoMatches()
        {
            const string query = "/opstenant?$expand=Buildings($filter=$this/Parameter eq 'FakeParameter')";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Empty(collection.First().Buildings);
                Assert.Empty(collection.Last().Buildings);                
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsNestedFilterEqUsingThisParameterWithMatches()
        {
            const string query = "/opstenant?$expand=Buildings($filter=$this/Name eq 'Two L1')&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
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
        public async Task OpsTenantExpandBuildingsFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantFilterGtDateNoExpand()
        {
            string query = "/opstenant?$filter=CreatedDate gt 2012-11-11T12:00:00.00Z";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantFilterLtDateNoExpand()
        {
            string query = "/opstenant?$filter=CreatedDate lt 2012-11-11T12:00:00.00Z";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Empty(collection);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsNoFilterAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OpsTenantNoExpandNoFilterNoOrderByShouldApplyByPk(bool alwaysSortByPk)
        {
            // Arrange
            const string query = "/opstenant";
            var querySettings = new QuerySettings
            {
                ODataSettings = new ODataSettings { AlwaysSortByPrimaryKey = alwaysSortByPk }
            };
            
            Test(Get<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings, customNamespace: null, enableLowerCamelCase : true));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));
            return;

            void Test(ICollection<OpsTenant> collection)
            {
                var expected = collection
                    .Select(x => x.Identity)
                    .OrderByDescending(identity => identity)
                    .ToList();
                
                if (alwaysSortByPk)
                {
                    Assert.True(collection
                    .Select(x => x.Identity)
                    .SequenceEqual(expected));
                }
                else
                {
                    Assert.False(collection
                        .Select(x => x.Identity)
                        .SequenceEqual(expected));
                }
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OpsTenantNoExpandNoFilterWithOrderByShouldApplyByPk(bool alwaysSortByPk)
        {
            const string query = "/opstenant?$orderby=Name desc";
            var querySettings = new QuerySettings
            {
                ODataSettings = new ODataSettings { AlwaysSortByPrimaryKey = alwaysSortByPk }
            };
            
            // Test multiple scenarios
            Test(Get<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));
            Test(await GetAsync<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query, GetMandators(), querySettings: querySettings));

            return;

            void Test(ICollection<OpsTenant> collection)
            {
                // Check if the collection is correctly ordered by Name (desc) and then by Identity (asc)
                var expected = collection
                    .OrderByDescending(x => x.Name)
                    .ThenByDescending(x => x.Identity)
                    .ToList();

                if (alwaysSortByPk)
                {
                    Assert.True(collection.SequenceEqual(expected),
                        "Collection is not ordered by Name (desc) and Identity (desc).");
                }
                else
                {
                    Assert.False(collection.SequenceEqual(expected),
                    "Collection is ordered by Name (desc) and Identity (desc).");
                }
            }
        }

        [Fact]
        public async Task OpsTenantNoExpandFilterEqAndOrderBy()
        {
            string query = "/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsSelectNameAndBuilderExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$select=Name&$expand=Buildings($select=Name,Builder;$expand=Builder($select=Name,City;$expand=City))&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
            Test(await GetUsingCustomNameSpace<OpsTenant, TMandator>(query));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Single(collection);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.NotEqual(default, collection.First().Buildings.First().Builder.City.Name);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            string query = "/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
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
        public async Task BuildingExpandBuilderTenantFilterEqAndOrderByWithParameter()
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
                await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>
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
                Assert.Single(collection);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Equal(buildingParameterValue, collection.First().Parameter);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(builderParameterValue, collection.First().Builder.Parameter);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantFilterEqAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
        public async Task BuildingExpandBuilderSelectNameExpandTenantFilterEqAndOrderBy()
        {
            string query = "/corebuilding?$top=5&$expand=Builder($select=Name),Tenant&$filter=Name eq 'One L1'";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, options));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
        public async Task BuildingSelectName_WithoutOrder_WithoutTop()
        {
            string query = "/corebuilding?$select=Name";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, querySettings: querySettings));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, querySettings: querySettings));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, querySettings: querySettings));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, querySettings: querySettings));
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, options, querySettings));
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
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query, querySettings: querySettings));
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
        public void BuildingsFilterNameDisableConstantParameterization()
        {
            string query = "/corebuilding?$filter=contains(Name, 'Two L2')";
            Test(GetQuery<CoreBuilding, TBuilding>(query, querySettings: new() { ODataSettings = new() { EnableConstantParameterization = false } }));

            void Test(IQueryable<CoreBuilding> queryable)
            {
                string sqlQuery = queryable.ToQueryString();
                Assert.Contains("LIKE N'%Two L2%'", sqlQuery);
                Assert.DoesNotContain("DECLARE", sqlQuery);
                Assert.DoesNotContain("ESCAPE", sqlQuery);
            }
        }

        [Fact]
        public void BuildingsFilterNameEnableConstantParameterization()
        {
            string query = "/corebuilding?$filter=contains(Name, 'Two L2')";
            Test(GetQuery<CoreBuilding, TBuilding>(query, querySettings: new() { ODataSettings = new() { EnableConstantParameterization = true } }));
            void Test(IQueryable<CoreBuilding> queryable)
            {
                string sqlQuery = queryable.ToQueryString();
                Assert.DoesNotContain("LIKE N'%Two L2%'", sqlQuery);
                Assert.Contains("DECLARE", sqlQuery);
                Assert.Contains("ESCAPE", sqlQuery);
            }
        }

        [Fact]
        public void BuildingsFilterNameDisableStableOrdering()
        {
            string query = "/corebuilding?$filter=contains(Name, 'Two L2')&$top=10";
            Test(GetQuery<CoreBuilding, TBuilding>(query, querySettings: new() { ODataSettings = new() { EnsureStableOrdering = false } }));

            void Test(IQueryable<CoreBuilding> queryable)
            {
                string sqlQuery = queryable.ToQueryString();
                Assert.Contains("TOP", sqlQuery);
                Assert.DoesNotContain("ORDER BY [o].[Identifier]", sqlQuery);
            }
        }

        [Fact]
        public void BuildingsFilterNameEnableStableOrdering()
        {
            string query = "/corebuilding?$filter=contains(Name, 'Two L2')&$top=10";
            Test(GetQuery<CoreBuilding, TBuilding>(query, querySettings: new() { ODataSettings = new() { EnsureStableOrdering = true } }));

            void Test(IQueryable<CoreBuilding> queryable)
            {
                string sqlQuery = queryable.ToQueryString();
                Assert.Contains("TOP", sqlQuery);
                Assert.Contains("ORDER BY [o].[Identifier]", sqlQuery);
            }
        }

        [Fact]
        public async Task OpsTenantOrderByCountOfReference()
        {
            string query = "/opstenant?$expand=Buildings&$orderby=Buildings/$count desc";
            Test(Get<OpsTenant, TMandator>(query));
            Test(await GetAsync<OpsTenant, TMandator>(query));
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
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
            Test(await GetAsyncUsingLowerCamelCase<OpsTenant, TMandator>(query));
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

        //ArgumentNullException: Value cannot be null. (Parameter 'bindings')
        //Microsoft.EntityFrameworkCore.InMemory (3.1.0).  Works using Microsoft.EntityFrameworkCore 3.1.
        /*
         *  StackTrace:
   at System.Linq.Expressions.Expression.ValidateMemberInitArgs(Type type, ReadOnlyCollection`1 bindings)
   at System.Linq.Expressions.Expression.MemberInit(NewExpression newExpression, IEnumerable`1 bindings)
   at System.Linq.Expressions.MemberInitExpression.Update(NewExpression newExpression, IEnumerable`1 bindings)
   at System.Linq.Expressions.ExpressionVisitor.VisitMemberInit(MemberInitExpression node)
   at System.Linq.Expressions.MemberInitExpression.Accept(ExpressionVisitor visitor)
   at System.Linq.Expressions.ExpressionVisitor.Visit(Expression node)
   at Microsoft.EntityFrameworkCore.InMemory.Query.Internal.InMemoryExpressionTranslatingExpressionVisitor.VisitConditional(ConditionalExpression conditionalExpression)
         */
        //[Fact]
        //public async Task CoreBuildingOrderByCountOfChildReferenceOfReference()
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
        public async Task CoreBuildingOrderByPropertyOfChildReferenceOfReference()
        {
            string query = "/corebuilding?$expand=Builder($expand=City)&$orderby=Builder/City/Name desc";
            Test(Get<CoreBuilding, TBuilding>(query));
            Test(await GetAsync<CoreBuilding, TBuilding>(query));
            Test(await GetAsyncUsingLowerCamelCase<CoreBuilding, TBuilding>(query));
            Test(await GetUsingCustomNameSpace<CoreBuilding, TBuilding>(query));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Leeds", collection.Last().Builder.City.Name);
            }
        }
        
        private IQueryable<TMandator> GetMandators()
        {
            return new TMandator[]
            {
                new TMandator
                {
                    Identity = Guid.Empty, // The first guide in order.
                    Name = "Two", // Duplicate name.
                    CreatedDate = new DateTime(2011, 12, 12)
                },
                new TMandator
                {
                    Identity = Guid.NewGuid(),
                    Name = "One",
                    CreatedDate = new DateTime(2012, 12, 12),
                    Buildings = new List<TBuilding>()
                },
                new TMandator
                {
                    Identity = Guid.NewGuid(),
                    Name = "Two",
                    CreatedDate = new DateTime(2012, 12, 12),
                    Buildings = new List<TBuilding>()
                }
            }.AsQueryable();
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
        public async Task FilteringOnRoot_AndChildCollection_WithMatches()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
        public async Task FilteringOnRoot_AndChildCollection_WithNoMatches()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
        public async Task FilteringOnRoot_AndChildCollection_WithNoMatches_SortRootAndChildCollection()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
        public async Task FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithMatches()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
        public async Task FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithNoMatches()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
        public async Task FilteringOnRoot_ChildCollection_WithTopNoOrderBy_AndChildCollectionOfChildCollection_WithNoMatches()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
                Assert.Single(collection.First().Products);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Count());
            }
        }

        [Fact]
        public async Task Filtering_On_Members_Not_Selected_In_Chiled_Collections()
        {
            string query = "/CategoryModel?$top=5&$expand=Products($select=ProductID;$filter=ProductName ne '';$top=1;$expand=AlternateAddresses($select=State;$filter=City ne ''))&$filter=CategoryName ne ''";
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
                Assert.Single(collection.First().Products);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Count());
            }
        }

        [Fact]
        public async Task FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithNoMatches_SortRoot_AndChildCollection_AndChildCollectionOfChildCollection()
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
                await GetAsyncUsingLowerCamelCase<CategoryModel, Category>
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
        public async Task SkipOnRootNoOrderByThenExpandAndSkipOnChildCollectionNoOrderByThenExpandSkipAndTopOnChildCollectionOfChildCollectionWithOrderBy()
        {
            const string query = "/CategoryModel?$skip=1&$expand=Products($skip=1;$expand=AlternateAddresses($skip=1;$top=3;$orderby=AddressID desc))";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().CategoryID);
                Assert.Single(collection.First().Products);
                Assert.Equal(5, collection.First().Products.First().ProductID);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Length);
                Assert.Equal(4, collection.First().Products.First().AlternateAddresses.First().AddressID);
                Assert.Equal(3, collection.First().Products.First().AlternateAddresses.Last().AddressID);
            }
        }

        [Fact]
        public async Task SkipOnRootNoOrderByThenExpandAndSkipOnChildCollectionNoOrderByThenExpandSkipAndTopOnChildCollectionOfChildCollectionNoOrderBy()
        {
            const string query = "/CategoryModel?$skip=1&$expand=Products($skip=1;$expand=AlternateAddresses($skip=1;$top=3))";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().CategoryID);
                Assert.Single(collection.First().Products);
                Assert.Equal(5, collection.First().Products.First().ProductID);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Length);
                Assert.Equal(4, collection.First().Products.First().AlternateAddresses.First().AddressID);
                Assert.Equal(5, collection.First().Products.First().AlternateAddresses.Last().AddressID);
            }
        }

        [Fact]
        public async Task ExpandChildCollectionWithTopAndSkipNoOrderBy()
        {
            const string query = "/CategoryModel?$expand=Products($skip=1;$top=2)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.First().Products.First().ProductID);
                Assert.Equal(3, collection.First().Products.Last().ProductID);
                Assert.Single(collection.Last().Products);
                Assert.Equal(5, collection.Last().Products.First().ProductID);
            }
        }

        [Fact]
        public async Task ExpandChildCollectionSkipBeyondAllElementsNoOrderBy()
        {
            const string query = "/CategoryModel?$expand=Products($skip=3)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Empty(collection.First().Products);
                Assert.Empty(collection.Last().Products);
            }
        }

        [Fact]
        public async Task ExpandChildCollectionWithSkipNoOrderByModelHasCompositeKey()
        {
            const string query = "/CategoryModel?$expand=CompositeKeys($skip=1)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
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
        public async Task ExpandChildCollectionWithSkipAndTopNoOrderByModelHasCompositeKey()
        {
            const string query = "/CategoryModel?$expand=CompositeKeys($skip=1;$top=2)";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
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
        public async Task SkipOnRootNoOrderBy()
        {
            const string query = "/CategoryModel?$skip=1";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().CategoryID);
            }
        }

        [Fact]
        public async Task SkipOnRootNoOrderByWithDuplicateEntities()
        {
            const string query = "/CategoryModel?$skip=1";
            Test(await GetAsyncDuplicateEntityName(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().CategoryID);
            }
        }

        [Fact]
        public async Task SkipOnRootNoOrderByWithDuplicateEntitiesAndCustomeNamespace()
        {
            const string query = "/CategoryModel?$skip=1";
            Test(await GetAsyncDuplicateEntityName(query, GetCategories(), null, null, "foo.bar"));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Single(collection);
                Assert.Equal(2, collection.First().CategoryID);
            }
        }

        [Fact]
        public async Task SkipBeyondAllElementsOnRootNoOrderBy()
        {
            const string query = "/CategoryModel?$skip=2";
            Test(await GetAsync<CategoryModel, Category>(query, GetCategories()));
            Test(await GetAsyncUsingLowerCamelCase<CategoryModel, Category>(query, GetCategories()));
            Test(await GetUsingCustomNameSpace<CategoryModel, Category>(query, GetCategories()));
            Test(Get<CategoryModel, Category>(query, GetCategories()));

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Empty(collection);
            }
        }

        private IQueryable<SuperCategory> GetSuperCategories()
         => new SuperCategory[]
            {
                new SuperCategory("SuperCategoryOne")
                {
                    SuperCategoryID = 1,
                    Categories = GetCategories()
                },
                new SuperCategory("SuperCategoryTwo")
                {
                    SuperCategoryID = 2,
                    Categories = Enumerable.Empty<Category>()
                }
            }.AsQueryable();

        [Fact]
        public async Task ExpandChildCollectionWithNoDefaultConstructorNoFilter()
        {
            const string query = "/SuperCategoryModel?$expand=Categories";
            Test(await GetAsync<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));
            Test(await GetAsyncUsingLowerCamelCase<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));
            Test(await GetUsingCustomNameSpace<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));
            Test(Get<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));

            static void Test(ICollection<SuperCategoryModel> collection)
            {
                Assert.NotEmpty(collection.First().Categories);
                Assert.Empty(collection.Last().Categories);
            }
        }

        [Fact]
        public async Task ExpandChildCollectionWithNoDefaultConstructorNestedFilter()
        {
            const string query = "/SuperCategoryModel?$expand=Categories($filter=CategoryName eq 'CategoryOne')";
            Test(await GetAsync<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));
            Test(await GetAsyncUsingLowerCamelCase<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));
            Test(await GetUsingCustomNameSpace<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));
            Test(Get<SuperCategoryModel, SuperCategory>(query, GetSuperCategories()));

            static void Test(ICollection<SuperCategoryModel> collection)
            {
                Assert.Single(collection.First().Categories);
                Assert.Empty(collection.Last().Categories);
            }
        }

        [Fact]
        public async Task CancellationThrowsException()
        {
            var cancelledToken = new CancellationTokenSource(TimeSpan.Zero).Token;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetAsync<CoreBuilding, TBuilding>("/corebuilding?$count=true", querySettings: new QuerySettings { AsyncSettings = new AsyncSettings { CancellationToken = cancelledToken } }));
        }

        [Fact]
        public async Task OrganizationIdentitiesWithNestedOrganizationNames()
        {
            string odataQuery = "/OrganizationIdentity?$select=Id&$expand=Organization($select=Id;$expand=Names($select=Id,LangId,Value;$filter=LangId eq 1))";
            var query = GetOrganizationIdentitiesWithNestedOrganizationNames();

            Assert.Equal(2, query.First().Organization.Names.Count);
            Assert.Single(query.First().Organization.Names, n => n.LangId == 1);

            var result = (await GetAsync<OrganizationIdentityDto, OrganizationIdentity>(odataQuery, query)).ToList();
            Assert.Single(result.First().Organization.Names);
        }

        private IQueryable<OrganizationIdentity> GetOrganizationIdentitiesWithNestedOrganizationNames()
        {
            var organization1 = new Organization() { Id = 1 };
            var name1 = new OrganizationName() { Id = 1, LangId = 1, Value = "Value (lang1)" };
            var name2 = new OrganizationName() { Id = 2, LangId = 2, Value = "Value (lang2)" };
            organization1.Names = new[] { name1, name2 };

            var identity1 = new OrganizationIdentity() { Id = 1, OrganizationId = organization1.Id, Organization = organization1, Value = "id1", TypeId = 1 };
            var identity2 = new OrganizationIdentity() { Id = 2, OrganizationId = organization1.Id, Organization = organization1, Value = "id2", TypeId = 2 };
            organization1.Identities = new[] { identity2 };

            return new[] { identity1, identity2 }.AsQueryable();
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

        private IQueryable<TModel> GetQuery<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
        {
            return DoGet
            (
                serviceProvider.GetRequiredService<IMapper>()
            );

            IQueryable<TModel> DoGet(IMapper mapper)
            {
                return serviceProvider.GetRequiredService<MyDbContext>().Set<TData>().GetQuery
                (
                    mapper,
                    options ?? GetODataQueryOptions<TModel>(query),
                    querySettings
                );
            };
        }

        private ICollection<TModel> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
        {
            return GetQuery<TModel, TData>(query, options, querySettings).ToList();
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, 
            IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null, bool enableLowerCamelCase = false) where TModel : class where TData : class
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
                    options ?? GetODataQueryOptions<TModel>(query, customNamespace, enableLowerCamelCase: enableLowerCamelCase),
                    querySettings
                );
            }
        }

        private Task<ICollection<TModel>> GetAsyncUsingLowerCamelCase<TModel, TData>(string query,
            IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
        {
            return GetAsync(query, dataQueryable, options, querySettings, customNamespace, true);
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

        private async Task<ICollection<TModel>> GetAsyncUsingLowerCamelCase<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
        {
            return await GetAsync
            (
                query,
                serviceProvider.GetRequiredService<MyDbContext>().Set<TData>(),
                options,
                querySettings,
                customNamespace,
                enableLowerCamelCase: true
            );
        }

        private async Task<ICollection<CategoryModel>> GetAsyncDuplicateEntityName(string query,
            IQueryable<Category> dataQueryable, ODataQueryOptions<Model.CategoryModel> options = null, QuerySettings querySettings = null, string customNamespace = null)
        {
            return
            (
                await DoGet
                (
                    serviceProvider.GetRequiredService<IMapper>()
                )
            ).ToList();

            async Task<IQueryable<Model.CategoryModel>> DoGet(IMapper mapper)
            {
                return await dataQueryable.GetQueryAsync
                (
                    mapper,
                    options ?? ODataHelpers.GetODataQueryOptionsWithDuplicateEntityName(query, serviceProvider, customNamespace),
                    querySettings
                );
            }
        }

        private Task<ICollection<TModel>> GetUsingCustomNameSpace<TModel, TData>(string query,
            IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class 
            => GetAsync(query, dataQueryable, options, querySettings, "com.FooBar");

        private Task<ICollection<TModel>> GetUsingCustomNameSpace<TModel, TData>(string query,
            ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class 
            => GetAsync<TModel, TData>(query, options, querySettings, "com.FooBar");

        private ODataQueryOptions<TModel> GetODataQueryOptions<TModel>(string query, string customNamespace = null, bool enableLowerCamelCase = false) where TModel : class
        {
            return ODataHelpers.GetODataQueryOptions<TModel>
            (
                query,
                serviceProvider,
                customNamespace,
                enableLowerCamelCase
            );
        }
    }

    public class GetQueryTestsFixture
    {
        public GetQueryTestsFixture()
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
                        @"Server=(localdb)\mssqllocaldb;Database=GetQueryTestsDatabase;ConnectRetryCount=0",
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
