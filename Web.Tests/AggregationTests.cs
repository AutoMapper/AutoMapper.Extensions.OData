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
    // Examples can be found at https://learn.microsoft.com/en-us/odata/client/grouping-and-aggregation
    public class AggregationTests
    {
        public AggregationTests()
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
        [InlineData("16324")]
        [InlineData("16325")]
        public async void AverageOnSalesAmount(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Amount with average as AverageAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().AverageAmount.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void SumOnSalesAmount(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Amount with sum as SumAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(24, collection.First().SumAmount.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MinOnSalesAmount(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Amount with min as MinAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(1, collection.First().MinAmount.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MaxOnSalesAmount(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Amount with max as MaxAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(8, collection.First().MaxAmount.ToObject<decimal>());
            }
        }

        [Theory(Skip = "Nested complex properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void AverageOnProductTaxRate(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Product/TaxRate with average as AverageProduct_TaxRate)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(0.1, collection.First().AverageProduct_TaxRate.ToObject<decimal>());
            }
        }

        private async Task<ICollection<TModel>> Get<TModel>(string query, string port)
        {
            HttpResponseMessage result = await clientFactory.CreateClient().GetAsync
            (
                $"http://localhost:{port}{query}"
            );

            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<OData<TModel>>(content).Value;
        }
    }
}
