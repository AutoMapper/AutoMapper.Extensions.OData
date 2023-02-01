using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

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

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
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

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void SumOnProductTaxRate(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Product/TaxRate with sum as SumProduct_TaxRate)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(0.8, collection.First().SumProduct_TaxRate.ToObject<decimal>());
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MinOnProductTaxRate(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Product/TaxRate with min as MinProduct_TaxRate)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(0.06, collection.First().MinProduct_TaxRate.ToObject<decimal>());
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MaxOnProductTaxRate(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Product/TaxRate with max as MaxProduct_TaxRate)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(0.14, collection.First().MaxProduct_TaxRate.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void CountDistinctOnProductId(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(ProductId with countdistinct as CountDistinctProductId)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().CountDistinctProductId.ToObject<decimal>());
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void CountDistinctOnProductColor(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Product/Color with countdistinct as CountDistinctProductColor)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(2, collection.First().CountDistinctProductId.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void FilterTransformationOnAverageUSDAmount(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=filter(CurrencyCode eq 'USD')/aggregate(Amount with average as AverageAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3.8m, collection.First().AverageAmount.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupByConstant(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(1, collection.Count);
                Assert.Equal(3, collection.First().AverageAmount.ToObject<decimal>());
                Assert.Equal(24, collection.First().SumAmount.ToObject<decimal>());
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupBySingleProperty(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((ProductId),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount))", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(3, collection.Count);
                foreach (var item in collection)
                {
                    switch (item.ProductId.ToObject<string>())
                    {
                        case "P1":
                            Assert.Equal(2, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(4, item.SumAmount.ToObject<decimal>());
                            break;

                        case "P2":
                            Assert.Equal(6, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(12, item.SumAmount.ToObject<decimal>());
                            break;

                        case "P3":
                            Assert.Equal(2, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(8, item.SumAmount.ToObject<decimal>());
                            break;

                        default:
                            throw new XunitException("Unexpected 'ProductId' found");
                    }
                }
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupBySingleNavigationProperty(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((Product/Category/Id),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount))", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(2, collection.Count);
                foreach (var item in collection)
                {
                    switch (item.CategoryId.ToObject<string>())
                    {
                        case "PG1":
                            Assert.Equal(2, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(4, item.SumAmount.ToObject<decimal>());
                            break;

                        case "PG2":
                            Assert.Equal(6, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(12, item.SumAmount.ToObject<decimal>());
                            break;

                        default:
                            throw new XunitException("Unexpected 'CategoryId' found");
                    }
                }
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupByMultipleProperties(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((ProductId,CurrencyCode),\r\naggregate(Amount with average as AverageAmount,Amount with sum as SumAmount))", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(5, collection.Count);
                foreach (var item in collection)
                {
                    switch (item.ProductId.ToObject<string>())
                    {
                        case "P1":
                            if (item.CurrencyCode.ToObject<string>() == "EUR")
                            {
                                Assert.Equal(2m, item.AverageAmount.ToObject<decimal>());
                                Assert.Equal(2m, item.SumAmount.ToObject<decimal>());
                            }
                            else if (item.CurrencyCode.ToObject<string>() == "USD")
                            {
                                Assert.Equal(2m, item.AverageAmount.ToObject<decimal>());
                                Assert.Equal(2m, item.SumAmount.ToObject<decimal>());
                            }
                            else
                            {
                                throw new XunitException("Unexpected 'CurrencyCode' found");
                            }
                            break;

                        case "P2":
                            if (item.CurrencyCode.ToObject<string>() == "USD")
                            {
                                Assert.Equal(6m, item.AverageAmount.ToObject<decimal>());
                                Assert.Equal(12m, item.SumAmount.ToObject<decimal>());
                            }
                            else
                            {
                                throw new XunitException("Unexpected 'CurrencyCode' found");
                            }
                            break;

                        case "P3":
                            if (item.CurrencyCode.ToObject<string>() == "EUR")
                            {
                                Assert.Equal(1.5m, item.AverageAmount.ToObject<decimal>());
                                Assert.Equal(3m, item.SumAmount.ToObject<decimal>());
                            }
                            else if (item.CurrencyCode.ToObject<string>() == "USD")
                            {
                                Assert.Equal(2.5m, item.AverageAmount.ToObject<decimal>());
                                Assert.Equal(5m, item.SumAmount.ToObject<decimal>());
                            }
                            else
                            {
                                throw new XunitException("Unexpected 'CurrencyCode' found");
                            }
                            break;

                        default:
                            throw new XunitException("Unexpected 'ProductId' found");
                    }
                }
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupByMultipleNavigationProperties(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((Product/CategoryId,Customer/Country),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount))", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void AggregatingNavigationProperty(string port)
        {
            Test(await Get<dynamic>("/Sales?$groupby((Customer/Country),aggregate(Product/TaxRate with min as MinTaxRate,Product/TaxRate with max as MaxTaxRate))", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupByAndCountOnNavigationProperty(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((ProductId,Customer/Country),aggregate($count as NumberOfProductsSoldByCountry))", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void GroupByAndCountDistinctOnNavigationProperty(string port)
        {
            Test(await Get<dynamic>("Sales?$apply=groupby((Customer/Country),aggregate(Product/Name with countdistinct as DistinctNumberOfProductsSoldByCountry))", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MemberInitializationInTheResultSelector(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((ProductId,Customer/Country),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount,Amount with min as MinAmount,Amount with max as MaxAmount,$count as NumberOfProductsSoldByCountry,Product/Name with countdistinct as DistinctNumberOfProductsSoldByCountry))", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MemberInitializationInTheKeySelector(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((Product/CategoryId,Customer/Country),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount))", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void MixedConstructorAndMemberInitializationInTheResultSelector(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((Time/Year,Product/Category/Name),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory(Skip = "Navigation properties cannot be part of the '$apply'-option (not yet supported)")]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void SupportForKnownPrimitiveTypesMemberAccessAndMethodCalls(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=groupby((Time/Year,Product/Category/Name,CurrencyCode),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount,Amount with min as MinAmount)", port));

            static void Test(ICollection<dynamic> collection)
            {
                throw new XunitException("Structure currently unknown because call is not supported yet!");
            }
        }

        [Theory]
        [InlineData("16324")]
        [InlineData("16325")]
        public async void FilterBeforeGrouping(string port)
        {
            Test(await Get<dynamic>("/Sales?$apply=filter(CurrencyCode eq 'USD')/groupby((ProductId),aggregate(Amount with average as AverageAmount,Amount with sum as SumAmount))", port));

            static void Test(ICollection<dynamic> collection)
            {
                Assert.Equal(3, collection.Count);
                foreach (var item in collection)
                {
                    switch (item.ProductId.ToObject<string>())
                    {
                        case "P1":
                            Assert.Equal(2m, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(2m, item.SumAmount.ToObject<decimal>());
                            break;

                        case "P2":
                            Assert.Equal(6m, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(12m, item.SumAmount.ToObject<decimal>());
                            break;

                        case "P3":
                            Assert.Equal(2.5m, item.AverageAmount.ToObject<decimal>());
                            Assert.Equal(5m, item.SumAmount.ToObject<decimal>());
                            break;

                        default:
                            throw new XunitException("Unexpected 'ProductId' found");
                    }
                }
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
