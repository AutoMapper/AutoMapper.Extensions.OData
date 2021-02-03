using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.AspNet.OData;
using AutoMapper.OData.Queryable.Tests.Data;
using AutoMapper.OData.Queryable.Tests.Model;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IDataContext = AutoMapper.OData.Queryable.Tests.Data.IDataContext;

namespace AutoMapper.OData.Queryable.Tests
{
    public class GetQueryTests : TestsBase
    {
        [Fact]
        public void IsConfigurationValid()
        {
            serviceProvider.GetRequiredService<IConfigurationProvider>().AssertConfigurationIsValid();
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsFilterEqAndOrderBy()
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
        public async Task OpsTenantExpandBuildingsFilterNeAndOrderBy()
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
        public async Task OpsTenantFilterEqNoExpand()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$filter=Name eq 'One'"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantFilterGtDateNoExpand()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate gt 2012-11-11T00:00:00.00Z"));

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
            Test(await Get<OpsTenant, TMandator>("/opstenant?$filter=CreatedDate lt 2012-11-11T12:00:00.00Z"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(0, collection.Count);
            }
        }

        [Fact]
        public async Task OpsTenantExpandBuildingsNoFilterAndOrderBy()
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
        public async Task OpsTenantNoExpandNoFilterAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Fact]
        public async Task OpsTenantNoExpandFilterEqAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc"));

            void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Fact]//Similar to test below but works if $select=Buildings is added to the query
        public async Task OpsTenantExpandBuildingsSelectNameAndBuilderExpandBuilderExpandCityFilterNeAndOrderBy()
        {
            Test(await Get<OpsTenant, TMandator>("/opstenant?$top=5&$select=Buildings,Name&$expand=Buildings($select=Name,Builder;$expand=Builder($select=Name,City;$expand=City))&$filter=Name ne 'One'&$orderby=Name desc"));

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
        public async Task OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy()
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
        public async Task BuildingExpandBuilderTenantFilterEqAndOrderByWithParameter()
        {
            string buildingParameterValue = Guid.NewGuid().ToString();
            int builderParameterValue = new Random().Next();
            var parameters = new
            {
                buildingParameter = buildingParameterValue,
                builderParameter = builderParameterValue
            };

            Test
            (
                await Get<CoreBuilding, TBuilding>
                (
                    "/corebuilding?$top=1&$expand=Builder&$filter=name eq 'One L1'",
                    null,
                    new QuerySettings
                    {
                        ProjectionSettings = new ProjectionSettings { Parameters = parameters },
                        ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False }
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
        public async Task BuildingExpandBuilderTenantFilterEqAndOrderBy()
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
        public async Task BuildingExpandBuilderSelectNameExpandTenantFilterEqAndOrderBy()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($select=Name),Tenant&$filter=name eq 'One L1'"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantFilterOnNestedPropertyAndOrderBy()
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
        public async Task BuildingExpandBuilderTenantExpandCityFilterOnPropertyAndOrderBy()
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
        public async Task BuildingExpandBuilderTenantExpandCityFilterOnNestedNestedPropertyWithCount()
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
        public async Task BuildingExpandBuilderTenantExpandCityOrderByName()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityOrderByNameThenByIdentity()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Fact]
        public async Task BuildingExpandBuilderTenantExpandCityOrderByBuilderName()
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
        public async Task BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1WithCount()
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
        public async Task BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1NoCount()
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
        public async Task BuildingSelectName_WithoutOrder_WithoutTop()
        {
            Test(await Get<CoreBuilding, TBuilding>("/corebuilding?$select=Name"));

            void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Fact]
        public async Task OpsTenantOrderByCountOfReference()
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

        [Fact]
        public async Task CoreBuildingOrderByPropertyOfChildReferenceOfReference()
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
                            AlternateAddresses = new Address[0],
                            SupplierAddress = new Address { City = "B" }
                        }
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
                            AlternateAddresses = new Address[0],
                            SupplierAddress = new Address { City = "C" }
                        }
                    }
                }
            }.AsQueryable();

        [Fact]
        public async Task FilteringOnRoot_AndChildCollection_WithMatches()
        {
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=ProductName eq 'ProductOne')&$filter=CategoryName eq 'CategoryOne'",
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
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$orderby=ProductName desc)&$filter=CategoryName ne ''&$orderby=CategoryName asc",
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(2, collection.First().Products.Count);
            }
        }

        [Fact]
        public async Task FilteringOnRoot_AndChildCollection_WithNoMatches_SortRootAndChildCollection()
        {
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$orderby=ProductName desc)&$filter=CategoryName ne ''&$orderby=CategoryName asc",
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
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=ProductName eq 'ProductOne';$expand=AlternateAddresses($filter=City eq 'CityOne'))&$filter=CategoryName eq 'CategoryOne'",
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
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$expand=AlternateAddresses($filter=City ne ''))&$filter=CategoryName ne ''",
                    GetCategories()
                )
            );

            static void Test(ICollection<CategoryModel> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(2, collection.First().Products.Count);
                Assert.Equal(2, collection.First().Products.First().AlternateAddresses.Count());
            }
        }

        [Fact]
        public async Task FilteringOnRoot_ChildCollection_WithTopNoOrderBy_AndChildCollectionOfChildCollection_WithNoMatches()
        {
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=ProductName ne '';$top=1;$expand=AlternateAddresses($filter=City ne ''))&$filter=CategoryName ne ''",
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
        public async Task FilteringOnRoot_ChildCollection_AndChildCollectionOfChildCollection_WithNoMatches_SortRoot_AndChildCollection_AndChildCollectionOfChildCollection()
        {
            Test
            (
                await Get<CategoryModel, Category>
                (
                    "/CategoryModel?$top=5&$expand=Products($filter=SupplierAddress/City ne '';$orderby=ProductName;$expand=AlternateAddresses($filter=City ne '';$orderby=City desc),SupplierAddress)&$filter=CategoryName ne ''&$orderby=CategoryName asc",
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

        private async Task<ICollection<TModel>> Get<TModel, TData>(string query, IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
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
                    options ?? ODataHelpers.GetODataQueryOptions<TModel>
                    (
                        query,
                        serviceProvider,
                        serviceProvider.GetRequiredService<IRouteBuilder>()
                    ),
                    querySettings
                );
            }
        }

        private Task<ICollection<TModel>> Get<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null) where TModel : class where TData : class
            => Get
            (
                query,
                serviceProvider.GetRequiredService<IDataContext>().Set<TData>(),
                options,
                querySettings
            );
    }
}
