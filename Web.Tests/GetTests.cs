using Domain.OData;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Web.Tests
{
    public class GetTests
    {
        public GetTests()
        {
            Initialize();
        }

        private IServiceProvider serviceProvider;
        private IHttpClientFactory clientFactory;

        private void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient();
            serviceProvider = services.BuildServiceProvider();

            clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        }
        
        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantSearchAndFilterNoResult(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$search=One&$filter=Name eq 'Two'", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(0, collection.Count);
            }
        }
        
        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantSearchAndFilterExpand(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$search=One&$filter=CreatedDate gt 2012-11-11T00:00:00.00Z&$expand=Buildings", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }
        
        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantSearchExpand(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$search=One&$expand=Buildings", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantSearchNoExpand(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$search=One", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.False(collection.First()?.Buildings?.Any() == true);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsFilterNeAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantFilterEqNoExpand(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$filter=Name eq 'One'", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.False(collection.First()?.Buildings?.Any() == true);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantFilterGtDateNoExpand(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$filter=CreatedDate gt 2012-11-11T00:00:00.00Z", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Null(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantFilterLtDateNoExpand(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$filter=CreatedDate lt 2012-11-11T12:00:00.00Z", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(0, collection.Count);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsNoFilterAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$expand=Buildings&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantNoExpandNoFilterAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.False(collection.First()?.Buildings?.Any() == true);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantNoExpandFilterEqAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$filter=Name eq 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.False(collection.First()?.Buildings?.Any() == true);
                Assert.Equal("One", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsSelectNameAndBuilderExpandBuilderExpandCityFilterNeAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$select=Name&$expand=Buildings($select=Name,Builder;$expand=Builder($select=Name,City;$expand=City))&$filter=Name ne 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.NotEqual(default, collection.First().Buildings.First().Builder.City.Name);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantFilterEqAndOrderBy(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=name eq 'One L1'", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantFilterOnNestedPropertyAndOrderBy(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder,Tenant&$filter=Builder/Name eq 'Sam'&$orderby=Name asc", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal("One", collection.First().Tenant.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityFilterOnPropertyAndOrderBy(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Name ne 'One L2'&$orderby=Name desc", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(4, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("Two L3", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityFilterOnNestedNestedPropertyWithCount(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=Builder/City/Name eq 'Leeds'&$count=true", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityOrderByName(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityOrderByNameThenByIdentity(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("Leeds", collection.First().Builder.City.Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderName(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$orderby=Builder/Name", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Two L1", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1WithCount(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$skip=4&$top=1&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity&$count=true", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderTenantExpandCityOrderByBuilderNameSkip3Take1NoCount(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$skip=4&$top=1&$expand=Builder($expand=City),Tenant&$orderby=Name desc,Identity", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingSelectName_WithoutOrder_WithoutTop(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$select=Name", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantOrderByCountOfReference(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$expand=Buildings&$orderby=Buildings/$count desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.NotNull(collection.First().Buildings);
                Assert.Equal("Two", collection.First().Name);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal(2, collection.Last().Buildings.Count);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantOrderByFilteredCount(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$expand=Buildings&$orderby=Buildings/$count($filter=Name eq 'One L1') desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.NotNull(collection.First().Buildings);
                Assert.Equal("One", collection.First().Name);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal(3, collection.Last().Buildings.Count);
            }
        }

        [Theory]
        [InlineData("16324")]
        //EF 6 seems to have a problem with circular reference Building/Tenant/Buildings
        //[InlineData("16325")]
        public async void CoreBuildingOrderByCountOfChildReferenceOfReference(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$expand=Tenant($expand=Buildings)&$orderby=Tenant/Buildings/$count desc", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Tenant.Buildings);
                Assert.Equal(3, collection.First().Tenant.Buildings.Count);
                Assert.Equal(2, collection.Last().Tenant.Buildings.Count);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void CoreBuildingOrderByPropertyOfChildReferenceOfReference(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$expand=Builder($expand=City)&$orderby=Builder/City/Name desc", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(5, collection.Count);
                Assert.NotNull(collection.First().Builder.City);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal("Leeds", collection.Last().Builder.City.Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantSelectNameExpandBuildings(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$select=Name&$expand=Buildings&$orderby=Name", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(2, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("One", collection.First().Name);
                Assert.Equal(default, collection.First().Identity);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsFilterEqAndOrderBy_FirstBuildingHasValues(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$select=Buildings&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.NotNull(collection.First().Buildings.First().Name);
                Assert.NotEqual(default, collection.First().Buildings.First().Identity);
                Assert.Equal(default, collection.First().Identity);
                Assert.Null(collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingSelectNameExpandBuilder_BuilderNameShouldBeSam(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$select=Name&$expand=Builder($select=Name)&$filter=name eq 'One L1'", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Null(collection.First().Builder.City);
                Assert.Null(collection.First().Tenant);
                Assert.Equal("One L1", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderSelectNamefilterEqAndOrderBy(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($select=Name)&$filter=name eq 'One L1'", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Null(collection.First().Builder.City);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Null(collection.First().Tenant);
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void BuildingExpandBuilderSelectNameExpandCityFilterEqAndOrderBy_CityShouldBeExpanded_BuilderNameShouldBeSam_BuilderIdShouldBeZero(string port)
        {
            Test(await Get<CoreBuilding>("/corebuilding?$top=5&$expand=Builder($select=Name;$expand=City)&$filter=name eq 'One L1'", port));

            static void Test(ICollection<CoreBuilding> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal("Sam", collection.First().Builder.Name);
                Assert.Equal(default, collection.First().Builder.Id);
                Assert.Equal("London", collection.First().Builder.City.Name);
                Assert.Equal(1, collection.First().Builder.City.Id);
                Assert.Equal("One L1", collection.First().Name);
                Assert.Null(collection.First().Tenant);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsSelectNameAndBuilderExpandBuilderExpandCityFilterNeAndOrderBy_filterAndSortChildCollection(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$select=Name&$expand=Buildings($filter=Name ne 'Two L1';$orderby=Name;$select=Name,Builder;$expand=Builder($select=Name,City;$expand=City))&$filter=Name ne 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().Buildings.Count);
                Assert.Equal("Two L2", collection.First().Buildings.First().Name);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.NotEqual(default, collection.First().Buildings.First().Builder.City.Name);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        [Theory]
        [InlineData("16324", "WithoutEnableQuery")]
        [InlineData("16324", "WithEnableQuery")]
        [InlineData("16325", "WithoutEnableQuery")]
        [InlineData("16325", "WithEnableQuery")]
        public async void OpsTenantExpandBuildingsExpandBuilderExpandCityFilterNeAndOrderBy_filterAndSortChildCollection(string port, string segment)
        {
            Test(await Get<OpsTenant>($"/opstenant/{segment}?$top=5&$expand=Buildings($filter=Name ne '';$orderby=Name desc;$expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc", port));

            static void Test(ICollection<OpsTenant> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().Buildings.Count);
                Assert.Equal("Two L3", collection.First().Buildings.First().Name);
                Assert.NotNull(collection.First().Buildings.First().Builder);
                Assert.NotNull(collection.First().Buildings.First().Builder.City);
                Assert.Equal("Two", collection.First().Name);
            }
        }

        private async Task<ICollection<TModel>> Get<TModel>(string query, string port)
        {
            HttpResponseMessage result = await clientFactory.CreateClient().GetAsync
            (
                $"http://localhost:{port}{query}"
            );

            result.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<OData<TModel>>(await result.Content.ReadAsStringAsync()).Value;
        }
    }

    public class OData<T>
    {
        public List<T> Value { get; set; }
    }
}
