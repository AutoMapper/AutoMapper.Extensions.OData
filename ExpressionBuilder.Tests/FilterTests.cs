//Adapted from Microsoft.AspNet.OData.Test.Query.Expressions.FilterBinderTests
using AutoMapper.AspNet.OData;
using ExpressionBuilder.Tests.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ExpressionBuilder.Tests
{
    public class FilterTests
    {
        public FilterTests()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        private void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

            serviceProvider = services.BuildServiceProvider();
        }

        #region Inequalities
        [Theory]
        [InlineData(null, true)]
        [InlineData("", false)]
        [InlineData("Doritos", false)]
        public void EqualityOperatorWithNull(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("ProductName eq null");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == null)");
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Doritos", true)]
        public void EqualityOperator(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("ProductName eq 'Doritos'");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == \"Doritos\")");
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("Doritos", false)]
        public void NotEqualOperator(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("ProductName ne 'Doritos'");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName != \"Doritos\")");
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(5.01, true)]
        [InlineData(4.99, false)]
        public void GreaterThanOperator(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice gt 5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice > Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(5.0, true)]
        [InlineData(4.99, false)]
        public void GreaterThanEqualOperator(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice ge 5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice >= Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(4.99, true)]
        [InlineData(5.01, false)]
        public void LessThanOperator(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice lt 5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice < Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(5.0, true)]
        [InlineData(5.01, false)]
        public void LessThanOrEqualOperator(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice le 5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NegativeNumbers()
        {
            //act
            var filter = GetFilter<Product>("UnitPrice le -5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(44m) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", -5.0));
            Assert.False(result);
        }

        [Theory]
        [InlineData("DateTimeOffsetProp eq DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp ne DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp != $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp ge DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp >= $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp le DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp <= $it.DateTimeOffsetProp)")]
        public void DateTimeOffsetInequalities(string clause, string expectedExpression)
        {
            //act
            var filter = GetFilter<DataTypes>(clause);

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }

        [Theory]
        [InlineData("DateTimeProp eq DateTimeProp", "$it => ($it.DateTimeProp == $it.DateTimeProp)")]
        [InlineData("DateTimeProp ne DateTimeProp", "$it => ($it.DateTimeProp != $it.DateTimeProp)")]
        [InlineData("DateTimeProp ge DateTimeProp", "$it => ($it.DateTimeProp >= $it.DateTimeProp)")]
        [InlineData("DateTimeProp le DateTimeProp", "$it => ($it.DateTimeProp <= $it.DateTimeProp)")]
        public void DateInEqualities(string clause, string expectedExpression)
        {
            //act
            var filter = GetFilter<DataTypes>(clause);

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }
        #endregion Inequalities

        #region Logical Operators
        [Fact]
        public void BooleanOperatorNullableTypes()
        {
            //act
            var filter = GetFilter<Product>("UnitPrice eq 5.00m or CategoryID eq 0");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (($it.UnitPrice == Convert(5.00)) OrElse ($it.CategoryID == 0))");
        }

        [Fact]
        public void BooleanComparisonOnNullableAndNonNullableType()
        {
            //act
            var filter = GetFilter<Product>("Discontinued eq true");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Discontinued == Convert(True))");
        }

        [Fact]
        public void BooleanComparisonOnNullableType()
        {
            //act
            var filter = GetFilter<Product>("Discontinued eq Discontinued");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Discontinued == $it.Discontinued)");
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(5.0, 0, true)]
        [InlineData(null, 1, false)]
        public void OrOperator(object unitPrice, object unitsInStock, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice eq 5.00m or UnitsInStock eq 0");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) OrElse (Convert($it.UnitsInStock) == Convert({1})))", 5.0, 0));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(5.0, 10, true)]
        [InlineData(null, 1, false)]
        public void AndOperator(object unitPrice, object unitsInStock, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice eq 5.00m and UnitsInStock eq 10.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) AndAlso (Convert($it.UnitsInStock) == Convert({1:0.00})))", 5.0, 10.0));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(5.0, false)]
        [InlineData(5.5, true)]
        public void Negation(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("not (UnitPrice eq 5.00m)");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => Not(($it.UnitPrice == Convert({0:0.00})))", 5.0));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void BoolNegation(bool discontinued, bool expected)
        {
            //act
            var filter = GetFilter<Product>("not Discontinued");
            bool result = RunFilter(filter, new Product { Discontinued = discontinued });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => Convert(Not($it.Discontinued))");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NestedNegation()
        {
            //act
            var filter = GetFilter<Product>("not (not(not    (Discontinued)))");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => Convert(Not(Not(Not($it.Discontinued))))");
        }
        #endregion Logical Operators

        #region Arithmetic Operators
        [Theory]
        [InlineData(null, false)]
        [InlineData(5.0, true)]
        [InlineData(15.01, false)]
        public void Subtraction(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("UnitPrice sub 1.00m lt 5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Addition()
        {
            //act
            var filter = GetFilter<Product>("UnitPrice add 1.00m lt 5.00m");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice + Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));
        }

        [Fact]
        public void Multiplication()
        {
            //act
            var filter = GetFilter<Product>("UnitPrice mul 1.00m lt 5.00m");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice * Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));
        }

        [Fact]
        public void Division()
        {
            //act
            var filter = GetFilter<Product>("UnitPrice div 1.00m lt 5.00m");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice / Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));
        }

        [Fact]
        public void Modulo()
        {
            //act
            var filter = GetFilter<Product>("UnitPrice mod 1.00m lt 5.00m");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice % Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));
        }
        #endregion Arithmetic Operators

        #region NULL  handling
        [Theory]
        [InlineData("UnitsInStock eq UnitsOnOrder", null, null, true)]
        [InlineData("UnitsInStock ne UnitsOnOrder", null, null, false)]
        [InlineData("UnitsInStock gt UnitsOnOrder", null, null, false)]
        [InlineData("UnitsInStock ge UnitsOnOrder", null, null, false)]
        [InlineData("UnitsInStock lt UnitsOnOrder", null, null, false)]
        [InlineData("UnitsInStock le UnitsOnOrder", null, null, false)]
        [InlineData("(UnitsInStock add UnitsOnOrder) eq UnitsInStock", null, null, true)]
        [InlineData("(UnitsInStock sub UnitsOnOrder) eq UnitsInStock", null, null, true)]
        [InlineData("(UnitsInStock mul UnitsOnOrder) eq UnitsInStock", null, null, true)]
        [InlineData("(UnitsInStock div UnitsOnOrder) eq UnitsInStock", null, null, true)]
        [InlineData("(UnitsInStock mod UnitsOnOrder) eq UnitsInStock", null, null, true)]
        [InlineData("UnitsInStock eq UnitsOnOrder", 1, null, false)]
        [InlineData("UnitsInStock eq UnitsOnOrder", 1, 1, true)]
        public void NullHandling(string filterString, object unitsInStock, object unitsOnOrder, bool expected)
        {
            //act
            var filter = GetFilter<Product>(filterString);
            bool result = RunFilter(filter, new Product { UnitsInStock = ToNullable<short>(unitsInStock), UnitsOnOrder = ToNullable<short>(unitsOnOrder) });

            //assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("UnitsInStock eq null", null, true)]
        [InlineData("UnitsInStock ne null", null, false)]
        public void NullHandling_LiteralNull(string filterString, object unitsInStock, bool expected)
        {
            //act
            var filter = GetFilter<Product>(filterString);
            bool result = RunFilter(filter, new Product { UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            Assert.Equal(expected, result);
        }
        #endregion NULL  handling

        #region Using the This Literal
        [Fact]
        public void NestedSelectFilter_IntegerCollection_GreaterThanAndLessThanOperator()
        {
            var filter = GetSelectNestedFilter<Product, int>("AlternateIDs($filter=$this gt 5 and $this lt 100)");

            AssertFilterStringIsCorrect(filter, "$it => (($it > 5) AndAlso ($it < 100))");

            int[] values = new int[] { 1, 2, 3, 4, 5, 6, 200 };
            Assert.Equal(new int[] { 6 }, values.Where(filter.Compile()).ToArray());
        }

        [Fact]
        public void NestedSelectFilter_IntegerCollection_GreaterThanOperator()
        {
            var filter = GetSelectNestedFilter<Product, int>("AlternateIDs($filter=$this gt 5)");

            AssertFilterStringIsCorrect(filter, "$it => ($it > 5)");

            int[] values = new int[] { 1, 2, 3, 4, 5, 6, 200 };
            Assert.Equal(new int[] { 6, 200 }, values.Where(filter.Compile()).ToArray());
        }

        [Fact]
        public void NestedExpandFilter_EntityCollection_EqualsOperator()
        {
            var filter = GetExpandNestedFilter<Product, Address>("AlternateAddresses($filter=$this/City eq 'Redmond')");

            AssertFilterStringIsCorrect(filter, "$it => ($it.City == \"Redmond\")");
        }


        #endregion

        [Theory]
        [InlineData("indexof('hello', StringProp) gt UIntProp")]
        [InlineData("indexof('hello', StringProp) gt ULongProp")]
        [InlineData("indexof('hello', StringProp) gt UShortProp")]
        [InlineData("indexof('hello', StringProp) gt NullableUShortProp")]
        [InlineData("indexof('hello', StringProp) gt NullableUIntProp")]
        [InlineData("indexof('hello', StringProp) gt NullableULongProp")]
        public void ComparisonsInvolvingCastsAndNullableValues(string filterString)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new DataTypes()));
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData("not doritos", 0, true)]
        [InlineData("Doritos", 1, false)]
        public void Grouping(string productName, object unitsInStock, bool expected)
        {
            //act
            var filter = GetFilter<Product>("((ProductName ne 'Doritos') or (UnitPrice lt 5.00m))");
            bool result = RunFilter(filter, new Product { ProductName = productName, UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.ProductName != \"Doritos\") OrElse ($it.UnitPrice < Convert({0:0.00})))", 5.0));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MemberExpressions()
        {
            //act
            var filter = GetFilter<Product>("Category/CategoryName eq 'Snacks'");
            bool result = RunFilter(filter, new Product { Category = new Category { CategoryName = "Snacks" } });

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.CategoryName == \"Snacks\")");
            Assert.True(result);
        }

        [Fact]
        public void MemberExpressionsRecursive()
        {
            //act
            var filter = GetFilter<Product>("Category/Product/Category/CategoryName eq 'Snacks'");

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.Product.Category.CategoryName == \"Snacks\")");
        }

        [Fact]
        public void ComplexPropertyNavigation()
        {
            //act
            var filter = GetFilter<Product>("SupplierAddress/City eq 'Redmond'");
            bool result = RunFilter(filter, new Product { SupplierAddress = new Address { City = "Redmond" } });

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => ($it.SupplierAddress.City == \"Redmond\")");
            Assert.True(result);
        }

        #region Any/All
        [Fact]
        public void AnyOnNavigationEnumerableCollections()
        {
            //act
            var filter = GetFilter<Product>("Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')");

            bool result1 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" },
                            new Product { ProductName = "NonSnacks" }
                        }
                    }
                }
            );

            bool result2 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "NonSnacks" }
                        }
                    }
                }
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result1);
            Assert.False(result2);
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/any(P: P/ProductName eq 'Snacks')");

            bool result1 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        QueryableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" },
                            new Product { ProductName = "NonSnacks" }
                        }.AsQueryable()
                    }
                }
            );

            bool result2 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        QueryableProducts = new Product[]
                        {
                            new Product { ProductName = "NonSnacks" }
                        }.AsQueryable()
                    }
                }
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result1);
            Assert.False(result2);
        }

        [Theory]
        [InlineData("Category/QueryableProducts/any(P: P/ProductID in (1))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Int32].Contains(P.ProductID))")]
        [InlineData("Category/EnumerableProducts/any(P: P/ProductID in (1))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Int32].Contains(P.ProductID))")]
        [InlineData("Category/QueryableProducts/any(P: P/GuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Guid].Contains(P.GuidProperty))")]
        [InlineData("Category/EnumerableProducts/any(P: P/GuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Guid].Contains(P.GuidProperty))")]
        [InlineData("Category/QueryableProducts/any(P: P/NullableGuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(P.NullableGuidProperty))")]
        [InlineData("Category/EnumerableProducts/any(P: P/NullableGuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(P.NullableGuidProperty))")]
        [InlineData("Category/QueryableProducts/any(P: P/Discontinued in (false, null))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(P.Discontinued))")]
        [InlineData("Category/EnumerableProducts/any(P: P/Discontinued in (false, null))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(P.Discontinued))")]
        public void AnyInOnNavigation(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("Category/QueryableProducts/any(P: false)", "$it => $it.Category.QueryableProducts.Any(P => False)")]
        [InlineData("Category/QueryableProducts/any(P: false and P/ProductName eq 'Snacks')", "$it => $it.Category.QueryableProducts.Any(P => (False AndAlso (P.ProductName == \"Snacks\")))")]
        [InlineData("Category/QueryableProducts/any(P: true)", "$it => $it.Category.QueryableProducts.Any()")]
        public void AnyOnNavigation_Contradiction(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Fact]
        public void AnyOnNavigation_NullCollection()
        {
            //act
            var filter = GetFilter<Product>("Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')");
            bool result = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" }
                        }
                    }
                }
            );

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new Product { Category = new Category { } }));
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result);
        }

        [Fact]
        public void AllOnNavigation_NullCollection()
        {
            //act
            var filter = GetFilter<Product>("Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')");
            bool result = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" }
                        }
                    }
                }
            );

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new Product { Category = new Category { } }));
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result);
        }

        [Fact]
        public void MultipleAnys_WithSameRangeVariableName()
        {
            //act
            var filter = GetFilter<Product>("AlternateIDs/any(n: n eq 42) and AlternateAddresses/any(n : n/City eq 'Redmond')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.AlternateIDs.Any(n => (n == 42)) AndAlso $it.AlternateAddresses.Any(n => (n.City == \"Redmond\")))");
        }

        [Fact]
        public void MultipleAlls_WithSameRangeVariableName()
        {
            //act
            var filter = GetFilter<Product>("AlternateIDs/all(n: n eq 42) and AlternateAddresses/all(n : n/City eq 'Redmond')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.AlternateIDs.All(n => (n == 42)) AndAlso $it.AlternateAddresses.All(n => (n.City == \"Redmond\")))");
        }

        [Fact]
        public void AnyOnNavigationEnumerableCollections_EmptyFilter()
        {
            //act
            var filter = GetFilter<Product>("Category/EnumerableProducts/any()");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Any()");
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections_EmptyFilter()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/any()");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.Any()");
        }

        [Fact]
        public void AllOnNavigationEnumerableCollections()
        {
            //act
            var filter = GetFilter<Product>("Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))");
        }

        [Fact]
        public void AllOnNavigationQueryableCollections()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/all(P: P/ProductName eq 'Snacks')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\"))");
        }

        [Fact]
        public void AnyInSequenceNotNested()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/any(P: P/ProductName eq 'Snacks') or Category/QueryableProducts/any(P2: P2/ProductName eq 'Snacks')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.Any(P2 => (P2.ProductName == \"Snacks\")))");
        }

        [Fact]
        public void AllInSequenceNotNested()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/all(P: P/ProductName eq 'Snacks') or Category/QueryableProducts/all(P2: P2/ProductName eq 'Snacks')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.All(P2 => (P2.ProductName == \"Snacks\")))");
        }

        [Fact]
        public void AnyOnPrimitiveCollection()
        {
            //act
            var filter = GetFilter<Product>("AlternateIDs/any(id: id eq 42)");

            bool result1 = RunFilter
            (
                filter,
                new Product { AlternateIDs = new[] { 1, 2, 42 } }
            );

            bool result2 = RunFilter
            (
                filter,
                new Product { AlternateIDs = new[] { 1, 2 } }
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateIDs.Any(id => (id == 42))");
            Assert.True(result1);
            Assert.False(result2);
        }

        [Fact]
        public void AllOnPrimitiveCollection()
        {
            //act
            var filter = GetFilter<Product>("AlternateIDs/all(id: id eq 42)");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateIDs.All(id => (id == 42))");
        }

        [Fact]
        public void AnyOnComplexCollection()
        {
            //act
            var filter = GetFilter<Product>("AlternateAddresses/any(address: address/City eq 'Redmond')");
            bool result = RunFilter
            (
                filter,
                new Product { AlternateAddresses = new[] { new Address { City = "Redmond" } } }
            );

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateAddresses.Any(address => (address.City == \"Redmond\"))");
            Assert.True(result);
        }

        [Fact]
        public void AllOnComplexCollection()
        {
            //act
            var filter = GetFilter<Product>("AlternateAddresses/all(address: address/City eq 'Redmond')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateAddresses.All(address => (address.City == \"Redmond\"))");
        }

        [Fact]
        public void RecursiveAllAny()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/all(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.All(P => P.Category.EnumerableProducts.Any(PP => (PP.ProductName == \"Snacks\")))");
        }
        #endregion Any/All

        #region String Functions
        [Theory]
        [InlineData("Abcd", 0, "Abcd", true)]
        [InlineData("Abcd", 1, "bcd", true)]
        [InlineData("Abcd", 3, "d", true)]
        [InlineData("Abcd", 4, "", true)]
        public void StringSubstringStart(string productName, int startIndex, string compareString, bool expected)
        {
            //act
            var filter = GetFilter<Product>(string.Format("substring(ProductName, {0}) eq '{1}'", startIndex, compareString));
            bool result = RunFilter
            (
                filter,
                new Product { ProductName = productName }
            );

            //assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Abcd", -1, "Abcd")]
        [InlineData("Abcd", 5, "")]
        public void StringSubstringStartOutOfRange(string productName, int startIndex, string compareString)
        {
            //act
            var filter = GetFilter<Product>(string.Format("substring(ProductName, {0}) eq '{1}'", startIndex, compareString));

            //assert
            Assert.Throws<ArgumentOutOfRangeException>(() => RunFilter(filter, new Product { ProductName = productName }));
        }

        [Theory]
        [InlineData("Abcd", 0, 1, "A", true)]
        [InlineData("Abcd", 0, 4, "Abcd", true)]
        [InlineData("Abcd", 0, 3, "Abc", true)]
        [InlineData("Abcd", 1, 3, "bcd", true)]
        [InlineData("Abcd", 2, 1, "c", true)]
        [InlineData("Abcd", 3, 1, "d", true)]
        public void StringSubstringStartAndLength(string productName, int startIndex, int length, string compareString, bool expected)
        {
            //act
            var filter = GetFilter<Product>(string.Format("substring(ProductName, {0}, {1}) eq '{2}'", startIndex, length, compareString));
            bool result = RunFilter
            (
                filter,
                new Product { ProductName = productName }
            );

            //assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Abcd", -1, 4, "Abcd")]
        [InlineData("Abcd", -1, 3, "Abc")]
        [InlineData("Abcd", 0, 5, "Abcd")]
        [InlineData("Abcd", 1, 5, "bcd")]
        [InlineData("Abcd", 4, 1, "")]
        [InlineData("Abcd", 0, -1, "")]
        [InlineData("Abcd", 5, -1, "")]
        public void StringSubstringStartAndLengthOutOfRange(string productName, int startIndex, int length, string compareString)
        {
            //act
            var filter = GetFilter<Product>(string.Format("substring(ProductName, {0}, {1}) eq '{2}'", startIndex, length, compareString));

            //assert
            Assert.Throws<ArgumentOutOfRangeException>(() => RunFilter(filter, new Product { ProductName = productName }));
        }

        [Theory]
        [InlineData("Abcd", true)]
        [InlineData("Abd", false)]
        public void StringContains(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("contains(ProductName, 'Abc')");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.Contains(\"Abc\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringContainsNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("contains(ProductName, 'Abc')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.Contains(\"Abc\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData("Abcd", true)]
        [InlineData("Abd", false)]
        public void StringStartsWith(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("startswith(ProductName, 'Abc')");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.StartsWith(\"Abc\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringStartsWithNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("startswith(ProductName, 'Abc')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.StartsWith(\"Abc\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData("AAbc", true)]
        [InlineData("Abcd", false)]
        public void StringEndsWith(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("endswith(ProductName, 'Abc')");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.EndsWith(\"Abc\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringEndsWithNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("endswith(ProductName, 'Abc')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.EndsWith(\"Abc\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData("AAbc", true)]
        [InlineData("", false)]
        public void StringLength(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("length(ProductName) gt 0");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Length > 0)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringLengthNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("length(ProductName) gt 0");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Length > 0)");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData("12345Abc", true)]
        [InlineData("1234Abc", false)]
        public void StringIndexOf(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("indexof(ProductName, 'Abc') eq 5");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.IndexOf(\"Abc\") == 5)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringIndexOfNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("indexof(ProductName, 'Abc') eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.IndexOf(\"Abc\") == 5)");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData("123uctName", true)]
        [InlineData("1234Abc", false)]
        public void StringSubstring(string productName, bool expected)
        {
            //act
            var filter1 = GetFilter<Product>("substring(ProductName, 3) eq 'uctName'");
            var filter2 = GetFilter<Product>("substring(ProductName, 3, 4) eq 'uctN'");
            bool result = RunFilter(filter1, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => ($it.ProductName.Substring(3) == \"uctName\")");
            AssertFilterStringIsCorrect(filter2, "$it => ($it.ProductName.Substring(3, 4) == \"uctN\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringSubstringNullReferenceException()
        {
            //act
            var filter1 = GetFilter<Product>("substring(ProductName, 3) eq 'uctName'");
            var filter2 = GetFilter<Product>("substring(ProductName, 3, 4) eq 'uctN'");

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => ($it.ProductName.Substring(3) == \"uctName\")");
            AssertFilterStringIsCorrect(filter2, "$it => ($it.ProductName.Substring(3, 4) == \"uctN\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter1, new Product { }));
        }

        [Theory]
        [InlineData("Tasty Treats", true)]
        [InlineData("Tasty Treatss", false)]
        public void StringToLower(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("tolower(ProductName) eq 'tasty treats'");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToLower() == \"tasty treats\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringToLowerNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("tolower(ProductName) eq 'tasty treats'");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToLower() == \"tasty treats\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData("Tasty Treats", true)]
        [InlineData("Tasty Treatss", false)]
        public void StringToUpper(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("toupper(ProductName) eq 'TASTY TREATS'");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToUpper() == \"TASTY TREATS\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringToUpperNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("toupper(ProductName) eq 'TASTY TREATS'");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToUpper() == \"TASTY TREATS\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(" Tasty Treats  ", true)]
        [InlineData(" Tasty Treatss  ", false)]
        public void StringTrim(string productName, bool expected)
        {
            //act
            var filter = GetFilter<Product>("trim(ProductName) eq 'Tasty Treats'");
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Trim() == \"Tasty Treats\")");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringTrimNullReferenceException()
        {
            //act
            var filter = GetFilter<Product>("trim(ProductName) eq 'Tasty Treats'");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Trim() == \"Tasty Treats\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
        }

        [Fact]
        public void StringConcat()
        {
            //act
            var filter = GetFilter<Product>("concat('Food', 'Bar') eq 'FoodBar'");
            bool result = RunFilter(filter, new Product { });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (\"Food\".Concat(\"Bar\") == \"FoodBar\")");
            Assert.True(result);
        }
        #endregion String Functions

        #region Date Functions
        [Fact]
        public void DateDay()
        {
            //act
            var filter = GetFilter<Product>("day(DiscontinuedDate) eq 8");
            bool result = RunFilter(filter, new Product { DiscontinuedDate = new DateTime(2000, 10, 8) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Day == 8)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
            Assert.True(result);
        }

        [Fact]
        public void DateDayNonNullable()
        {
            //act
            var filter = GetFilter<Product>("day(NonNullableDiscontinuedDate) eq 8");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.NonNullableDiscontinuedDate.Day == 8)");
        }

        [Fact]
        public void DateMonth()
        {
            //act
            var filter = GetFilter<Product>("month(DiscontinuedDate) eq 8");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Month == 8)");
        }

        [Fact]
        public void DateYear()
        {
            //act
            var filter = GetFilter<Product>("year(DiscontinuedDate) eq 1974");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Year == 1974)");
        }

        [Fact]
        public void DateHour()
        {
            //act
            var filter = GetFilter<Product>("hour(DiscontinuedDate) eq 8");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Hour == 8)");
        }

        [Fact]
        public void DateMinute()
        {
            //act
            var filter = GetFilter<Product>("minute(DiscontinuedDate) eq 12");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Minute == 12)");
        }

        [Fact]
        public void DateSecond()
        {
            //act
            var filter = GetFilter<Product>("second(DiscontinuedDate) eq 33");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Second == 33)");
        }

        [Theory]
        [InlineData("year(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Year == 100)")]
        [InlineData("month(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Month == 100)")]
        [InlineData("day(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Day == 100)")]
        [InlineData("hour(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Hour == 100)")]
        [InlineData("minute(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Minute == 100)")]
        [InlineData("second(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Second == 100)")]
        [InlineData("now() eq 2016-11-08Z", "$it => (DateTimeOffset.UtcNow == 11/08/2016 00:00:00 +00:00)")]
        public void DateTimeOffsetFunctions(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("year(Birthday) eq 100", "$it => ({0}.Year == 100)")]
        [InlineData("month(Birthday) eq 100", "$it => ({0}.Month == 100)")]
        [InlineData("day(Birthday) eq 100", "$it => ({0}.Day == 100)")]
        [InlineData("hour(Birthday) eq 100", "$it => ({0}.Hour == 100)")]
        [InlineData("minute(Birthday) eq 100", "$it => ({0}.Minute == 100)")]
        [InlineData("second(Birthday) eq 100", "$it => ({0}.Second == 100)")]
        public void DateTimeFunctions(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, String.Format(expression, "$it.Birthday"));
        }

        [Theory]
        [InlineData("year(NullableDateProperty) eq 2015", "$it => ($it.NullableDateProperty.Value.Year == 2015)")]
        [InlineData("month(NullableDateProperty) eq 12", "$it => ($it.NullableDateProperty.Value.Month == 12)")]
        [InlineData("day(NullableDateProperty) eq 23", "$it => ($it.NullableDateProperty.Value.Day == 23)")]
        public void DateFunctions_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("year(NullableDateOnlyProperty) eq 2015", "$it => ($it.NullableDateOnlyProperty.Value.Year == 2015)")]
        [InlineData("month(NullableDateOnlyProperty) eq 12", "$it => ($it.NullableDateOnlyProperty.Value.Month == 12)")]
        [InlineData("day(NullableDateOnlyProperty) eq 23", "$it => ($it.NullableDateOnlyProperty.Value.Day == 23)")]
        public void DateOnlyFunctions_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("year(DateProperty) eq 2015", "$it => ($it.DateProperty.Year == 2015)")]
        [InlineData("month(DateProperty) eq 12", "$it => ($it.DateProperty.Month == 12)")]
        [InlineData("day(DateProperty) eq 23", "$it => ($it.DateProperty.Day == 23)")]
        public void DateFunctions_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("year(DateOnlyProperty) eq 2015", "$it => ($it.DateOnlyProperty.Year == 2015)")]
        [InlineData("month(DateOnlyProperty) eq 12", "$it => ($it.DateOnlyProperty.Month == 12)")]
        [InlineData("day(DateOnlyProperty) eq 23", "$it => ($it.DateOnlyProperty.Day == 23)")]
        public void DateOnlyFunctions_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("hour(NullableTimeOfDayProperty) eq 10", "$it => ($it.NullableTimeOfDayProperty.Value.Hours == 10)")]
        [InlineData("minute(NullableTimeOfDayProperty) eq 20", "$it => ($it.NullableTimeOfDayProperty.Value.Minutes == 20)")]
        [InlineData("second(NullableTimeOfDayProperty) eq 30", "$it => ($it.NullableTimeOfDayProperty.Value.Seconds == 30)")]
        public void TimeOfDayFunctions_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("hour(NullableTimeOnlyProperty) eq 10", "$it => ($it.NullableTimeOnlyProperty.Value.Hour == 10)")]
        [InlineData("minute(NullableTimeOnlyProperty) eq 20", "$it => ($it.NullableTimeOnlyProperty.Value.Minute == 20)")]
        [InlineData("second(NullableTimeOnlyProperty) eq 30", "$it => ($it.NullableTimeOnlyProperty.Value.Second == 30)")]
        public void TimeOnlyFunctions_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("hour(TimeOfDayProperty) eq 10", "$it => ($it.TimeOfDayProperty.Hours == 10)")]
        [InlineData("minute(TimeOfDayProperty) eq 20", "$it => ($it.TimeOfDayProperty.Minutes == 20)")]
        [InlineData("second(TimeOfDayProperty) eq 30", "$it => ($it.TimeOfDayProperty.Seconds == 30)")]
        public void TimeOfDayFunctions_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("hour(TimeOnlyProperty) eq 10", "$it => ($it.TimeOnlyProperty.Hour == 10)")]
        [InlineData("minute(TimeOnlyProperty) eq 20", "$it => ($it.TimeOnlyProperty.Minute == 20)")]
        [InlineData("second(TimeOnlyProperty) eq 30", "$it => ($it.TimeOnlyProperty.Second == 30)")]
        public void TimeOnlyFunctions_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("fractionalseconds(DiscontinuedDate) eq 0.2", "$it => ((Convert($it.DiscontinuedDate.Value.Millisecond) / 1000) == 0.2)")]
        [InlineData("fractionalseconds(NullableTimeOfDayProperty) eq 0.2", "$it => ((Convert($it.NullableTimeOfDayProperty.Value.Milliseconds) / 1000) == 0.2)")]
        [InlineData("fractionalseconds(NullableTimeOnlyProperty) eq 0.2", "$it => ((Convert($it.NullableTimeOnlyProperty.Value.Millisecond) / 1000) == 0.2)")]
        public void FractionalsecondsFunction_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("fractionalseconds(NonNullableDiscontinuedDate) eq 0.2", "$it => ((Convert($it.NonNullableDiscontinuedDate.Millisecond) / 1000) == 0.2)")]
        [InlineData("fractionalseconds(TimeOfDayProperty) eq 0.2", "$it => ((Convert($it.TimeOfDayProperty.Milliseconds) / 1000) == 0.2)")]
        [InlineData("fractionalseconds(TimeOnlyProperty) eq 0.2", "$it => ((Convert($it.TimeOnlyProperty.Millisecond) / 1000) == 0.2)")]
        public void FractionalsecondsFunction_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("date(DiscontinuedDate) eq 2015-02-26",
            "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))")]
        [InlineData("date(DiscontinuedDate) lt 2016-02-26",
            "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))")]
        [InlineData("2015-02-26 ge date(DiscontinuedDate)",
            "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day))")]
        [InlineData("null ne date(DiscontinuedDate)", "$it => (null != $it.DiscontinuedDate)")]
        [InlineData("date(DiscontinuedDate) eq null", "$it => ($it.DiscontinuedDate == null)")]
        public void DateFunction_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("date(NonNullableDiscontinuedDate) eq 2015-02-26",
            "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))")]
        [InlineData("date(NonNullableDiscontinuedDate) lt 2016-02-26",
            "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))")]
        [InlineData("2015-02-26 ge date(NonNullableDiscontinuedDate)",
            "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day))")]
        public void DateFunction_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("time(DiscontinuedDate) eq 01:02:03.0040000",
            "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("time(DiscontinuedDate) ge 01:02:03.0040000",
            "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("01:02:03.0040000 le time(DiscontinuedDate)",
            "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))))")]
        [InlineData("null ne time(DiscontinuedDate)", "$it => (null != $it.DiscontinuedDate)")]
        [InlineData("time(DiscontinuedDate) eq null", "$it => ($it.DiscontinuedDate == null)")]
        public void TimeFunction_Nullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }

        [Theory]
        [InlineData("time(NonNullableDiscontinuedDate) eq 01:02:03.0040000",
            "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("time(NonNullableDiscontinuedDate) ge 01:02:03.0040000",
            "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("01:02:03.0040000 le time(NonNullableDiscontinuedDate)",
            "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))))")]
        public void TimeFunction_NonNullable(string filterString, string expression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expression);
        }
        #endregion Date Functions

        #region Math Functions
        [Fact]
        public void RecursiveMethodCall()
        {
            //act
            var filter = GetFilter<Product>("floor(floor(UnitPrice)) eq 123m");
            bool result = RunFilter(filter, new Product { UnitPrice = 123.3m });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor().Floor() == 123)");
            Assert.True(result);
        }

        [Fact]
        public void RecursiveMethodCallInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("floor(floor(UnitPrice)) eq 123m");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor().Floor() == 123)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

        }

        [Fact]
        public void MathRoundDecimalInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("round(UnitPrice) gt 5.00m");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice.Value.Round() > {0:0.00})", 5.0));
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        public static IEnumerable<object[]> MathRoundDecimal_DataSet
            => new List<object[]>
                {
                    new object[] { 5.9m, true },
                    new object[] { 5.4m, false }
                };

        [Theory, MemberData(nameof(MathRoundDecimal_DataSet))]
        public void MathRoundDecimal(decimal? unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("round(UnitPrice) gt 5.00m");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice.Value.Round() > {0:0.00})", 5.0));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathRoundDoubleInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("round(Weight) gt 5d");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.Weight.Value.Round() > {0})", 5));
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(5.9d, true)]
        [InlineData(5.4d, false)]
        public void MathRoundDouble(double? weight, bool expected)
        {
            //act
            var filter = GetFilter<Product>("round(Weight) gt 5d");
            bool result = RunFilter(filter, new Product { Weight = ToNullable<double>(weight) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.Weight.Value.Round() > {0})", 5));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathRoundFloatInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("round(Width) gt 5f");

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (Convert($it.Width).Value.Round() > {0})", 5));
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(5.9f, true)]
        [InlineData(5.4f, false)]
        public void MathRoundFloat(float? width, bool expected)
        {
            //act
            var filter = GetFilter<Product>("round(Width) gt 5f");
            bool result = RunFilter(filter, new Product { Width = ToNullable<float>(width) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (Convert($it.Width).Value.Round() > {0})", 5));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathFloorDecimalInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("floor(UnitPrice) eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        public static IEnumerable<object[]> MathFloorDecimal_DataSet
            => new List<object[]>
                {
                    new object[] { 5.4m, true },
                    new object[] { 4.4m, false }
                };

        [Theory, MemberData(nameof(MathFloorDecimal_DataSet))]
        public void MathFloorDecimal(decimal? unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("floor(UnitPrice) eq 5");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor() == 5)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathFloorDoubleInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("floor(Weight) eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Floor() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(5.4d, true)]
        [InlineData(4.4d, false)]
        public void MathFloorDouble(double? weight, bool expected)
        {
            //act
            var filter = GetFilter<Product>("floor(Weight) eq 5");
            bool result = RunFilter(filter, new Product { Weight = ToNullable<double>(weight) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Floor() == 5)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathFloorFloatInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("floor(Width) eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Floor() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(5.4f, true)]
        [InlineData(4.4f, false)]
        public void MathFloorFloat(float? width, bool expected)
        {
            //act
            var filter = GetFilter<Product>("floor(Width) eq 5");
            bool result = RunFilter(filter, new Product { Width = ToNullable<float>(width) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Floor() == 5)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathCeilingDecimalInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("ceiling(UnitPrice) eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Ceiling() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        public static IEnumerable<object[]> MathCeilingDecimal_DataSet
            => new List<object[]>
                {
                    new object[] { 4.1m, true },
                    new object[] { 5.9m, false }
                };

        [Theory, MemberData(nameof(MathCeilingDecimal_DataSet))]
        public void MathCeilingDecimal(object unitPrice, bool expected)
        {
            //act
            var filter = GetFilter<Product>("ceiling(UnitPrice) eq 5");
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Ceiling() == 5)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathCeilingDoubleInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("ceiling(Weight) eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Ceiling() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(4.1d, true)]
        [InlineData(5.9d, false)]
        public void MathCeilingDouble(double? weight, bool expected)
        {
            //act
            var filter = GetFilter<Product>("ceiling(Weight) eq 5");
            bool result = RunFilter(filter, new Product { Weight = ToNullable<double>(weight) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Ceiling() == 5)");
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MathCeilingFloatInvalidOperationException()
        {
            //act
            var filter = GetFilter<Product>("ceiling(Width) eq 5");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Ceiling() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
        }

        [Theory]
        [InlineData(4.1f, true)]
        [InlineData(5.9f, false)]
        public void MathCeilingFloat(float? width, bool expected)
        {
            //act
            var filter = GetFilter<Product>("ceiling(Width) eq 5");
            bool result = RunFilter(filter, new Product { Width = ToNullable<float>(width) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Ceiling() == 5)");
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("floor(FloatProp) eq floor(FloatProp)")]
        [InlineData("round(FloatProp) eq round(FloatProp)")]
        [InlineData("ceiling(FloatProp) eq ceiling(FloatProp)")]
        [InlineData("floor(DoubleProp) eq floor(DoubleProp)")]
        [InlineData("round(DoubleProp) eq round(DoubleProp)")]
        [InlineData("ceiling(DoubleProp) eq ceiling(DoubleProp)")]
        [InlineData("floor(DecimalProp) eq floor(DecimalProp)")]
        [InlineData("round(DecimalProp) eq round(DecimalProp)")]
        [InlineData("ceiling(DecimalProp) eq ceiling(DecimalProp)")]
        public void MathFunctions_VariousTypes(string filterString)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);
            bool result = RunFilter(filter, new DataTypes { });

            //assert
            Assert.True(result);
        }
        #endregion Math Functions

        #region Custom Functions
        [Fact]
        public void CustomMethod_InstanceMethodOfDeclaringType()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction =
                    new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                     EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });
            const string padrightMethodName = "padright";

            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                CustomMethodCache.CacheCustomMethod(padrightMethodName, padRightStringMethodInfo);

                //act
                var filter = GetFilter<Product>(string.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName));
                bool result = RunFilter(filter, new Product { ProductName = productName });

                //assert
                Assert.True(result);
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(CustomMethodCache.RemoveCachedCustomMethod(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_InstanceMethodNotOfDeclaringType()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                        EdmCoreModel.Instance.GetString(true),
                        EdmCoreModel.Instance.GetString(true),
                        EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(FilterTests).GetMethod("PadRightInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            const string padrightMethodName = "padright";
            try
            {
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                CustomMethodCache.CacheCustomMethod(padrightMethodName, padRightStringMethodInfo);

                Assert.Throws<NotImplementedException>(() => GetFilter<Product>(string.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName)));
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(CustomMethodCache.RemoveCachedCustomMethod(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_StaticExtensionMethod()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(StringExtender).GetMethod("PadRightExStatic", BindingFlags.Public | BindingFlags.Static);

            const string padrightMethodName = "padright";
            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                CustomMethodCache.CacheCustomMethod(padrightMethodName, padRightStringMethodInfo);

                //act
                var filter = GetFilter<Product>(string.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName));
                bool result = RunFilter(filter, new Product { ProductName = productName });

                //assert
                Assert.True(result);
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(CustomMethodCache.RemoveCachedCustomMethod(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_StaticMethodNotOfDeclaringType()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(FilterTests).GetMethod("PadRightStatic", BindingFlags.NonPublic | BindingFlags.Static);

            const string padrightMethodName = "padright";
            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                CustomMethodCache.CacheCustomMethod(padrightMethodName, padRightStringMethodInfo);

                //act
                var filter = GetFilter<Product>(string.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName));
                bool result = RunFilter(filter, new Product { ProductName = productName });

                //assert
                Assert.True(result);
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(CustomMethodCache.RemoveCachedCustomMethod(padrightMethodName, padRightStringMethodInfo));
            }
        }
        #endregion Custom Functions

        #region Data Types
        [Fact]
        public void GuidExpression()
        {
            //act
            var filter1 = GetFilter<DataTypes>("GuidProp eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E");
            var filter2 = GetFilter<DataTypes>("GuidProp eq 0efdaecf-a9f0-42f3-a384-1295917af95e");

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");
            AssertFilterStringIsCorrect(filter2, "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");
        }

        [Theory]
        [InlineData("DateTimeProp eq 2000-12-12T12:00:00Z", "$it => ($it.DateTimeProp == {0})")]
        [InlineData("DateTimeProp lt 2000-12-12T12:00:00Z", "$it => ($it.DateTimeProp < {0})")]
        public void DateTimeExpression(string clause, string expectedExpression)
        {
            var dateTime = new DateTimeOffset(new DateTime(2000, 12, 12, 12, 0, 0), TimeSpan.Zero);
            //act
            var filter = GetFilter<DataTypes>(clause);

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, expectedExpression, dateTime));
        }

        [Theory]
        [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00Z'", "$it => ($it.DateTimeOffsetProp == {0})", 0)]
        [InlineData("DateTimeOffsetProp ge datetimeoffset'2002-10-10T17:00:00Z'", "$it => ($it.DateTimeOffsetProp >= {0})", 0)]
        [InlineData("DateTimeOffsetProp le datetimeoffset'2002-10-10T17:00:00-07:00'", "$it => ($it.DateTimeOffsetProp <= {0})", -7)]
        [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00-0600'", "$it => ($it.DateTimeOffsetProp == {0})", -6)]
        [InlineData("DateTimeOffsetProp lt datetimeoffset'2002-10-10T17:00:00-05'", "$it => ($it.DateTimeOffsetProp < {0})", -5)]
        [InlineData("DateTimeOffsetProp ne datetimeoffset'2002-10-10T17:00:00%2B09:30'", "$it => ($it.DateTimeOffsetProp != {0})", 9.5)]
        [InlineData("DateTimeOffsetProp gt datetimeoffset'2002-10-10T17:00:00%2B0545'", "$it => ($it.DateTimeOffsetProp > {0})", 5.75)]
        public void DateTimeOffsetExpression(string clause, string expectedExpression, double offsetHours)
        {
            var dateTimeOffset = new DateTimeOffset(2002, 10, 10, 17, 0, 0, TimeSpan.FromHours(offsetHours));

            // There's currently a bug here. For now, the test checks for the presence of the bug (as a reminder to fix
            // the test once the bug is fixed).
            // The following assert shows the behavior with the bug and should be removed once the bug is fixed.
            Assert.Throws<Microsoft.OData.ODataException>(() => GetFilter<DataTypes>(clause));

            // TODO: No DateTimeOffset parsing in ODataUriParser
            Assert.NotNull(expectedExpression);
            // The following call shows the behavior without the bug, and should be enabled once the bug is fixed.
            //VerifyQueryDeserialization<DataTypes>(
            //    "" + clause,
            //    String.Format(CultureInfo.InvariantCulture, expectedExpression, dateTimeOffset));
        }

        [Fact]
        public void IntegerLiteralSuffix()
        {
            //act
            var filter1 = GetFilter<DataTypes>("LongProp lt 987654321L and LongProp gt 123456789l");
            var filter2 = GetFilter<DataTypes>("LongProp lt -987654321L and LongProp gt -123456789l");

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => (($it.LongProp < 987654321) AndAlso ($it.LongProp > 123456789))");
            AssertFilterStringIsCorrect(filter2, "$it => (($it.LongProp < -987654321) AndAlso ($it.LongProp > -123456789))");
        }

        [Fact]
        public void EnumInExpression()
        {
            //act
            var filter = GetFilter<DataTypes>("SimpleEnumProp in ('First', 'Second')");
            var constant = (ConstantExpression)((MethodCallExpression)filter.Body).Arguments[0];
            var values = (IList<SimpleEnum>)constant.Value;

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[ExpressionBuilder.Tests.Data.SimpleEnum].Contains($it.SimpleEnumProp)");
            Assert.Equal(new[] { SimpleEnum.First, SimpleEnum.Second }, values);
        }

        [Fact]
        public void EnumInExpression_WithNullValue_Throws()
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>("SimpleEnumProp in ('First', null)"));
            Assert.Equal
            (
                "A null value was found with the expected type 'ExpressionBuilder.Tests.Data.SimpleEnum[Nullable=False]'. The expected type 'ExpressionBuilder.Tests.Data.SimpleEnum[Nullable=False]' does not allow null values.",
                exception.Message
            );
        }

        [Fact]
        public void EnumInExpression_NullableEnum_WithNullable()
        {
            //act
            var filter = GetFilter<DataTypes>("NullableSimpleEnumProp in ('First', 'Second')");
            var constant = (ConstantExpression)((MethodCallExpression)filter.Body).Arguments[0];
            var values = (IList<SimpleEnum?>)constant.Value;

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.Nullable`1[ExpressionBuilder.Tests.Data.SimpleEnum]].Contains($it.NullableSimpleEnumProp)");
            Assert.Equal(new SimpleEnum?[] { SimpleEnum.First, SimpleEnum.Second }, values);
        }

        [Fact]
        public void EnumInExpression_NullableEnum_WithNullValue()
        {
            //act
            var filter = GetFilter<DataTypes>("NullableSimpleEnumProp in ('First', null)");
            var constant = (ConstantExpression)((MethodCallExpression)filter.Body).Arguments[0];
            var values = (IList<SimpleEnum?>)constant.Value;

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.Nullable`1[ExpressionBuilder.Tests.Data.SimpleEnum]].Contains($it.NullableSimpleEnumProp)");
            Assert.Equal(new SimpleEnum?[] { SimpleEnum.First, null }, values);
        }

        [Fact]
        public void RealLiteralSuffixes()
        {
            //act
            var filter1 = GetFilter<DataTypes>("FloatProp lt 4321.56F and FloatProp gt 1234.56f");
            var filter2 = GetFilter<DataTypes>("DecimalProp lt 4321.56M and DecimalProp gt 1234.56m");

            //assert
            AssertFilterStringIsCorrect(filter1, string.Format(CultureInfo.InvariantCulture, "$it => (($it.FloatProp < {0:0.00}) AndAlso ($it.FloatProp > {1:0.00}))", 4321.56, 1234.56));
            AssertFilterStringIsCorrect(filter2, string.Format(CultureInfo.InvariantCulture, "$it => (($it.DecimalProp < {0:0.00}) AndAlso ($it.DecimalProp > {1:0.00}))", 4321.56, 1234.56));
        }

        [Theory]
        [InlineData("'hello,world'", "hello,world")]
        [InlineData("'''hello,world'", "'hello,world")]
        [InlineData("'hello,world'''", "hello,world'")]
        [InlineData("'hello,''wor''ld'", "hello,'wor'ld")]
        [InlineData("'hello,''''''world'", "hello,'''world")]
        [InlineData("'\"hello,world\"'", "\"hello,world\"")]
        [InlineData("'\"hello,world'", "\"hello,world")]
        [InlineData("'hello,world\"'", "hello,world\"")]
        [InlineData("'hello,\"world'", "hello,\"world")]
        [InlineData("'México D.F.'", "México D.F.")]
        [InlineData("'æææøøøååå'", "æææøøøååå")]
        [InlineData("'いくつかのテキスト'", "いくつかのテキスト")]
        public void StringLiterals(string literal, string expected)
        {
            //act
            var filter = GetFilter<Product>("ProductName eq " + literal);

            //assert
            AssertFilterStringIsCorrect(filter, string.Format("$it => ($it.ProductName == \"{0}\")", expected));
        }

        [Theory]
        [InlineData('$')]
        [InlineData('&')]
        [InlineData('+')]
        [InlineData(',')]
        [InlineData('/')]
        [InlineData(':')]
        [InlineData(';')]
        [InlineData('=')]
        [InlineData('?')]
        [InlineData('@')]
        [InlineData(' ')]
        [InlineData('<')]
        [InlineData('>')]
        [InlineData('#')]
        [InlineData('%')]
        [InlineData('{')]
        [InlineData('}')]
        [InlineData('|')]
        [InlineData('\\')]
        [InlineData('^')]
        [InlineData('~')]
        [InlineData('[')]
        [InlineData(']')]
        [InlineData('`')]
        public void SpecialCharactersInStringLiteral(char c)
        {
            //act
            var filter = GetFilter<Product>("ProductName eq '" + c + "'");
            bool result = RunFilter(filter, new Product { ProductName = c.ToString() });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format("$it => ($it.ProductName == \"{0}\")", c));
            Assert.True(result);
        }
        #endregion Data Types

        #region Casts
        [Fact]
        public void NSCast_OnEnumerableEntityCollection_GeneratesExpression_WithOfTypeOnEnumerable()
        {
            //act
            var filter = GetFilter<Product>("Category/EnumerableProducts/ExpressionBuilder.Tests.Data.DerivedProduct/any(p: p/ProductName eq 'ProductName')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.OfType().Any(p => (p.ProductName == \"ProductName\"))");
        }

        [Fact]
        public void NSCast_OnQueryableEntityCollection_GeneratesExpression_WithOfTypeOnQueryable()
        {
            //act
            var filter = GetFilter<Product>("Category/QueryableProducts/ExpressionBuilder.Tests.Data.DerivedProduct/any(p: p/ProductName eq 'ProductName')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.OfType().Any(p => (p.ProductName == \"ProductName\"))");
        }

        [Fact]
        public void NSCast_OnEntityCollection_CanAccessDerivedInstanceProperty()
        {
            //act
            var filter = GetFilter<Product>("Category/Products/ExpressionBuilder.Tests.Data.DerivedProduct/any(p: p/DerivedProductName eq 'DerivedProductName')");
            bool result1 = RunFilter(filter, new Product { Category = new Category { Products = new Product[] { new DerivedProduct { DerivedProductName = "DerivedProductName" } } } });
            bool result2 = RunFilter(filter, new Product { Category = new Category { Products = new Product[] { new DerivedProduct { DerivedProductName = "NotDerivedProductName" } } } });

            //assert
            Assert.True(result1);
            Assert.False(result2);
        }

        [Fact]
        public void NSCast_OnSingleEntity_GeneratesExpression_WithAsOperator()
        {
            //act
            var filter = GetFilter<DerivedProduct>("ExpressionBuilder.Tests.Data.Product/ProductName eq 'ProductName'");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (($it As Product).ProductName == \"ProductName\")");
        }

        [Theory]
        [InlineData("ExpressionBuilder.Tests.Data.Product/ProductName eq 'ProductName'")]
        [InlineData("ExpressionBuilder.Tests.Data.DerivedProduct/DerivedProductName eq 'DerivedProductName'")]
        [InlineData("ExpressionBuilder.Tests.Data.DerivedProduct/Category/CategoryID eq 123")]
        [InlineData("ExpressionBuilder.Tests.Data.DerivedProduct/Category/ExpressionBuilder.Tests.Data.DerivedCategory/CategoryID eq 123")]
        public void Inheritance_WithDerivedInstance(string filterString)
        {
            //act
            var filter = GetFilter<DerivedProduct>(filterString);
            bool result = RunFilter(filter, new DerivedProduct { Category = new DerivedCategory { CategoryID = 123 }, ProductName = "ProductName", DerivedProductName = "DerivedProductName" });

            //assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("ExpressionBuilder.Tests.Data.DerivedProduct/DerivedProductName eq 'ProductName'")]
        [InlineData("ExpressionBuilder.Tests.Data.DerivedProduct/Category/CategoryID eq 123")]
        [InlineData("ExpressionBuilder.Tests.Data.DerivedProduct/Category/ExpressionBuilder.Tests.Data.DerivedCategory/CategoryID eq 123")]
        public void Inheritance_WithBaseInstance(string filterString)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product()));

        }

        [Fact]
        public void CastToNonDerivedType_Throws()
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>("ExpressionBuilder.Tests.Data.DerivedCategory/CategoryID eq 123"));
            Assert.Equal
            (
                "Encountered invalid type cast. 'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'.",
                exception.Message
            );
        }

        [Theory]
        [InlineData("Edm.Int32 eq 123", "A binary operator with incompatible types was detected. Found operand types 'Edm.String' and 'Edm.Int32' for operator kind 'Equal'.")]
        [InlineData("ProductName/Edm.String eq 123", "A binary operator with incompatible types was detected. Found operand types 'Edm.String' and 'Edm.Int32' for operator kind 'Equal'.")]
        public void CastToNonEntityType_Throws(string filterString, string error)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                error,
                exception.Message
            );
        }

        [Theory]
        [InlineData("Edm.NonExistentType eq 123")]
        [InlineData("Category/Edm.NonExistentType eq 123")]
        [InlineData("Category/Products/Edm.NonExistentType eq 123")]
        public void CastToNonExistantType_Throws(string filterString)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                "The child type 'Edm.NonExistentType' in a cast was not an entity type. Casts can only be performed on entity types.",
                exception.Message
            );
        }
        #endregion Casts

        #region cast in query option
        [Theory]
        [InlineData("cast(null,Edm.Int16) eq null", "$it => (null == null)")]
        [InlineData("cast(null,Edm.Int32) eq 123", "$it => (null == Convert(123))")]
        [InlineData("cast(null,Edm.Int64) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Single) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Double) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Decimal) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Boolean) ne true", "$it => (null != Convert(True))")]
        [InlineData("cast(null,Edm.Byte) ne 1", "$it => (null != Convert(1))")]
        [InlineData("cast(null,Edm.Guid) eq 00000000-0000-0000-0000-000000000000", "$it => (null == Convert(00000000-0000-0000-0000-000000000000))")]
        [InlineData("cast(null,Edm.String) ne '123'", "$it => (null != \"123\")")]
        [InlineData("cast(null,Edm.DateTimeOffset) eq 2001-01-01T12:00:00.000+08:00", "$it => (null == Convert(01/01/2001 12:00:00 +08:00))")]
        [InlineData("cast(null,Edm.Duration) eq duration'P8DT23H59M59.9999S'", "$it => (null == Convert(8.23:59:59.9999000))")]
        [InlineData("cast(null,'ExpressionBuilder.Tests.Data.SimpleEnum') eq null", "$it => (null == null)")]
        [InlineData("cast(null,'ExpressionBuilder.Tests.Data.FlagsEnum') eq null", "$it => (null == null)")]
        [InlineData("cast(IntProp,Edm.String) eq '123'", "$it => ($it.IntProp.ToString() == \"123\")")]
        [InlineData("cast(LongProp,Edm.String) eq '123'", "$it => ($it.LongProp.ToString() == \"123\")")]
        [InlineData("cast(SingleProp,Edm.String) eq '123'", "$it => ($it.SingleProp.ToString() == \"123\")")]
        [InlineData("cast(DoubleProp,Edm.String) eq '123'", "$it => ($it.DoubleProp.ToString() == \"123\")")]
        [InlineData("cast(DecimalProp,Edm.String) eq '123'", "$it => ($it.DecimalProp.ToString() == \"123\")")]
        [InlineData("cast(BoolProp,Edm.String) eq '123'", "$it => ($it.BoolProp.ToString() == \"123\")")]
        [InlineData("cast(ByteProp,Edm.String) eq '123'", "$it => ($it.ByteProp.ToString() == \"123\")")]
        [InlineData("cast(GuidProp,Edm.String) eq '123'", "$it => ($it.GuidProp.ToString() == \"123\")")]
        [InlineData("cast(StringProp,Edm.String) eq '123'", "$it => ($it.StringProp == \"123\")")]
        [InlineData("cast(DateTimeOffsetProp,Edm.String) eq '123'", "$it => ($it.DateTimeOffsetProp.ToString() == \"123\")")]
        [InlineData("cast(TimeSpanProp,Edm.String) eq '123'", "$it => ($it.TimeSpanProp.ToString() == \"123\")")]
        [InlineData("cast(SimpleEnumProp,Edm.String) eq '123'", "$it => (Convert($it.SimpleEnumProp).ToString() == \"123\")")]
        [InlineData("cast(FlagsEnumProp,Edm.String) eq '123'", "$it => (Convert($it.FlagsEnumProp).ToString() == \"123\")")]
        [InlineData("cast(LongEnumProp,Edm.String) eq '123'", "$it => (Convert($it.LongEnumProp).ToString() == \"123\")")]
        [InlineData("cast(NullableIntProp,Edm.String) eq '123'", "$it => (IIF($it.NullableIntProp.HasValue, $it.NullableIntProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableLongProp,Edm.String) eq '123'", "$it => (IIF($it.NullableLongProp.HasValue, $it.NullableLongProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableSingleProp,Edm.String) eq '123'", "$it => (IIF($it.NullableSingleProp.HasValue, $it.NullableSingleProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableDoubleProp,Edm.String) eq '123'", "$it => (IIF($it.NullableDoubleProp.HasValue, $it.NullableDoubleProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableDecimalProp,Edm.String) eq '123'", "$it => (IIF($it.NullableDecimalProp.HasValue, $it.NullableDecimalProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableBoolProp,Edm.String) eq '123'", "$it => (IIF($it.NullableBoolProp.HasValue, $it.NullableBoolProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableByteProp,Edm.String) eq '123'", "$it => (IIF($it.NullableByteProp.HasValue, $it.NullableByteProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableGuidProp,Edm.String) eq '123'", "$it => (IIF($it.NullableGuidProp.HasValue, $it.NullableGuidProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableDateTimeOffsetProp,Edm.String) eq '123'", "$it => (IIF($it.NullableDateTimeOffsetProp.HasValue, $it.NullableDateTimeOffsetProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableTimeSpanProp,Edm.String) eq '123'", "$it => (IIF($it.NullableTimeSpanProp.HasValue, $it.NullableTimeSpanProp.Value.ToString(), null) == \"123\")")]
        [InlineData("cast(NullableSimpleEnumProp,Edm.String) eq '123'", "$it => (IIF($it.NullableSimpleEnumProp.HasValue, Convert($it.NullableSimpleEnumProp.Value).ToString(), null) == \"123\")")]
        [InlineData("cast(IntProp,Edm.Int64) eq 123", "$it => (Convert($it.IntProp) == 123)")]
        [InlineData("cast(NullableLongProp,Edm.Double) eq 1.23", "$it => (Convert($it.NullableLongProp) == 1.23)")]
        [InlineData("cast(2147483647,Edm.Int16) ne null", "$it => (Convert(Convert(2147483647)) != null)")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.SimpleEnum'1',Edm.String) eq '1'", "$it => (Convert(Second).ToString() == \"1\")")]
        [InlineData("cast(cast(cast(IntProp,Edm.Int64),Edm.Int16),Edm.String) eq '123'", "$it => (Convert(Convert($it.IntProp)).ToString() == \"123\")")]
        [InlineData("cast('123',ExpressionBuilder.Tests.Data.SimpleEnum) ne null", "$it => (Convert(123) != null)")]
        public void CastMethod_Succeeds(string filterString, string expectedResult)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expectedResult);
        }

        [Theory]
        [InlineData("cast(NoSuchProperty,Edm.Int32) ne null",
            "Could not find a property named 'NoSuchProperty' on type 'ExpressionBuilder.Tests.Data.DataTypes'.")]
        public void Cast_UndefinedSource_ThrowsODataException(string filterString, string errorMessage)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                errorMessage,
                exception.Message
            );
        }

        public static IEnumerable<object[]> CastToUnquotedUndefinedTarget
            => new List<object[]>
                {
                    new [] { "cast(Edm.DateTime) eq null", "Edm.DateTime" },
                    new [] { "cast(Edm.Unknown) eq null", "Edm.Unknown" },
                    new [] { "cast(null,Edm.DateTime) eq null", "Edm.DateTime" },
                    new [] { "cast(null,Edm.Unknown) eq null", "Edm.Unknown" },
                    new [] { "cast('2001-01-01T12:00:00.000',Edm.DateTime) eq null", "Edm.DateTime" },
                    new [] { "cast('2001-01-01T12:00:00.000',Edm.Unknown) eq null", "Edm.Unknown" },
                    new [] { "cast(DateTimeProp,Edm.DateTime) eq null", "Edm.DateTime" },
                    new [] { "cast(DateTimeProp,Edm.Unknown) eq null", "Edm.Unknown" }
                };

        [Theory]
        [MemberData(nameof(CastToUnquotedUndefinedTarget))]
        public void CastToUnquotedUndefinedTarget_ThrowsODataException(string filterString, string typeName)
        {
            // arrange
            var errorMessage = string.Format(
                "The child type '{0}' in a cast was not an entity type. Casts can only be performed on entity types.",
                typeName);

            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                errorMessage,
                exception.Message
            );
        }

        public static IEnumerable<object[]> CastToQuotedUndefinedTarget
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "cast('Edm.DateTime') eq null" },
                    new [] { "cast('Edm.Unknown') eq null" },
                    new [] { "cast(null,'Edm.DateTime') eq null" },
                    new [] { "cast(null,'Edm.Unknown') eq null" },
                    new [] { "cast('2001-01-01T12:00:00.000','Edm.DateTime') eq null" },
                    new [] { "cast('','Edm.Unknown') eq null" },
                    new [] { "cast(DateTimeProp,'Edm.DateTime') eq null" },
                    new [] { "cast(IntProp,'Edm.Unknown') eq null" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CastToQuotedUndefinedTarget))]
        public void CastToQuotedUndefinedTarget_ThrowsODataException(string filterString)
        {
            // arrange
            var errorMessage = "Cast or IsOf Function must have a type in its arguments.";

            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                errorMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData("cast(ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.FlagsEnum) ne null")]
        [InlineData("cast(0,ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(0,ExpressionBuilder.Tests.Data.FlagsEnum) ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.SimpleEnum'0',ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.FlagsEnum'0',ExpressionBuilder.Tests.Data.FlagsEnum) ne null")]
        [InlineData("cast(SimpleEnumProp,ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(FlagsEnumProp,ExpressionBuilder.Tests.Data.FlagsEnum) ne null")]
        [InlineData("cast(NullableSimpleEnumProp,ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(IntProp,ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(DateTimeOffsetProp,ExpressionBuilder.Tests.Data.SimpleEnum) ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.SimpleEnum'1',Edm.Int32) eq 1")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.FlagsEnum'1',Edm.Int32) eq 1")]
        [InlineData("cast(SimpleEnumProp,Edm.Int32) eq 123")]
        [InlineData("cast(FlagsEnumProp,Edm.Int32) eq 123")]
        [InlineData("cast(NullableSimpleEnumProp,Edm.Guid) ne null")]

        [InlineData("cast('ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast('ExpressionBuilder.Tests.Data.FlagsEnum') ne null")]
        [InlineData("cast(0,'ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast(0,'ExpressionBuilder.Tests.Data.FlagsEnum') ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.SimpleEnum'0','ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.FlagsEnum'0','ExpressionBuilder.Tests.Data.FlagsEnum') ne null")]
        [InlineData("cast(SimpleEnumProp,'ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast(FlagsEnumProp,'ExpressionBuilder.Tests.Data.FlagsEnum') ne null")]
        [InlineData("cast(NullableSimpleEnumProp,'ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast(IntProp,'ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast(DateTimeOffsetProp,'ExpressionBuilder.Tests.Data.SimpleEnum') ne null")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.SimpleEnum'1','Edm.Int32') eq 1")]
        [InlineData("cast(ExpressionBuilder.Tests.Data.FlagsEnum'1','Edm.Int32') eq 1")]
        [InlineData("cast(SimpleEnumProp,'Edm.Int32') eq 123")]
        [InlineData("cast(FlagsEnumProp,'Edm.Int32') eq 123")]
        [InlineData("cast(NullableSimpleEnumProp,'Edm.Guid') ne null")]
        public void Cast_UnsupportedSourceOrTargetForEnumCast_Throws(string filterString)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                "Enumeration type value can only be casted to or from string.",
                exception.Message
            );
        }

        [Theory]
        [InlineData("cast(IntProp,Edm.DateTimeOffset) eq null")]
        [InlineData("cast(ByteProp,Edm.Guid) eq null")]
        [InlineData("cast(NullableLongProp,Edm.Duration) eq null")]
        [InlineData("cast(StringProp,Edm.Double) eq null")]
        [InlineData("cast(DateTimeOffsetProp,Edm.Int32) eq null")]
        [InlineData("cast(NullableGuidProp,Edm.Int64) eq null")]
        [InlineData("cast(Edm.Int32) eq null")]
        [InlineData("cast($it,Edm.String) eq null")]
        [InlineData("cast(ComplexProp,Edm.Double) eq null")]
        [InlineData("cast(ComplexProp,Edm.String) eq null")]
        [InlineData("cast(StringProp,ExpressionBuilder.Tests.Data.SimpleEnum) eq null")]
        [InlineData("cast(StringProp,ExpressionBuilder.Tests.Data.FlagsEnum) eq null")]
        public void Cast_UnsupportedTarget_ReturnsNull(string filterString)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (null == null)");
        }

        [Theory]
        [InlineData("cast(StringProp,Edm.Int16) eq null")]//For Int16 the BinaryOperatorNode Converts to Int32 hence the Convert(null)
        public void Cast_UnsupportedTarget_ReturnsNull_Non_Default_Number(string filterString)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert(null) == null)");
        }

        [Theory]
        [InlineData("cast(null,ExpressionBuilder.Tests.Data.Address) ne null",
            "Encountered invalid type cast. " +
            "'ExpressionBuilder.Tests.Data.Address' is not assignable from 'ExpressionBuilder.Tests.Data.DataTypes'.")]
        [InlineData("cast(null,ExpressionBuilder.Tests.Data.DataTypes) ne null",
            "Cast or IsOf Function must have a type in its arguments.")]
        public void Cast_NonPrimitiveTarget_ThrowsODataException(string filterString, string expectErrorMessage)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                expectErrorMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData("cast(null,'Edm.Int32') ne null")]
        [InlineData("cast(StringProp,'ExpressionBuilder.Tests.Data.SimpleEnum') eq null")]
        [InlineData("cast(IntProp,'Edm.String') eq '123'")]
        [InlineData("cast('ExpressionBuilder.Tests.Data.DataTypes') eq null")]
        [InlineData("cast($it,'ExpressionBuilder.Tests.Data.DataTypes') eq null")]
        public void SingleQuotesOnTypeNameOfCast_WorksForNow(string filterString)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);

            //assert
            Assert.NotNull(filter);
        }

        [Fact]
        public void SingleQuotesOnEnumTypeNameOfCast_WorksForNow()
        {
            //act
            FilterClause filterClause = ODataHelpers.GetFilterClause<DataTypes>("cast(StringProp,'ExpressionBuilder.Tests.Data.SimpleEnum') eq null", serviceProvider);

            //assert
            var castNode = Assert.IsType<SingleValueFunctionCallNode>(((BinaryOperatorNode)filterClause.Expression).Left);
            Assert.Equal("cast", castNode.Name);
            Assert.Equal("ExpressionBuilder.Tests.Data.SimpleEnum", ((ConstantNode)castNode.Parameters.Last()).Value);
        }

        public static IEnumerable<object[]> CastToQuotedPrimitiveType
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "cast('Edm.Binary') eq null" },
                    new [] { "cast('Edm.Boolean') eq null" },
                    new [] { "cast('Edm.Byte') eq null" },
                    new [] { "cast('Edm.DateTimeOffset') eq null" },
                    new [] { "cast('Edm.Decimal') eq null" },
                    new [] { "cast('Edm.Double') eq null" },
                    new [] { "cast('Edm.Duration') eq null" },
                    new [] { "cast('Edm.Guid') eq null" },
                    new [] { "cast('Edm.Int16') eq null" },
                    new [] { "cast('Edm.Int32') eq null" },
                    new [] { "cast('Edm.Int64') eq null" },
                    new [] { "cast('Edm.SByte') eq null" },
                    new [] { "cast('Edm.Single') eq null" },
                    new [] { "cast('Edm.String') eq null" },

                    new [] { "cast(null,'Edm.Binary') eq null" },
                    new [] { "cast(null,'Edm.Boolean') eq null" },
                    new [] { "cast(null,'Edm.Byte') eq null" },
                    new [] { "cast(null,'Edm.DateTimeOffset') eq null" },
                    new [] { "cast(null,'Edm.Decimal') eq null" },
                    new [] { "cast(null,'Edm.Double') eq null" },
                    new [] { "cast(null,'Edm.Duration') eq null" },
                    new [] { "cast(null,'Edm.Guid') eq null" },
                    new [] { "cast(null,'Edm.Int16') eq null" },
                    new [] { "cast(null,'Edm.Int32') eq null" },
                    new [] { "cast(null,'Edm.Int64') eq null" },
                    new [] { "cast(null,'Edm.SByte') eq null" },
                    new [] { "cast(null,'Edm.Single') eq null" },
                    new [] { "cast(null,'Edm.String') eq null" },

                    new [] { "cast(binary'T0RhdGE=','Edm.Binary') eq binary'T0RhdGE='" },
                    new [] { "cast(false,'Edm.Boolean') eq false" },
                    new [] { "cast(23,'Edm.Byte') eq 23" },
                    new [] { "cast(2001-01-01T12:00:00.000+08:00,'Edm.DateTimeOffset') eq 2001-01-01T12:00:00.000+08:00" },
                    new [] { "cast(23,'Edm.Decimal') eq 23" },
                    new [] { "cast(23,'Edm.Double') eq 23" },
                    new [] { "cast(duration'PT12H','Edm.Duration') eq duration'PT12H'" },
                    new [] { "cast(00000000-0000-0000-0000-000000000000,'Edm.Guid') eq 00000000-0000-0000-0000-000000000000" },
                    new [] { "cast(23,'Edm.Int16') eq 23" },
                    new [] { "cast(23,'Edm.Int32') eq 23" },
                    new [] { "cast(23,'Edm.Int64') eq 23" },
                    new [] { "cast(23,'Edm.SByte') eq 23" },
                    new [] { "cast(23,'Edm.Single') eq 23" },
                    new [] { "cast('hello','Edm.String') eq 'hello'" },

                    new [] { "cast(ByteArrayProp,'Edm.Binary') eq null" },
                    new [] { "cast(BoolProp,'Edm.Boolean') eq true" },
                    new [] { "cast(DateTimeOffsetProp,'Edm.DateTimeOffset') eq 2001-01-01T12:00:00.000+08:00" },
                    new [] { "cast(DecimalProp,'Edm.Decimal') eq 23" },
                    new [] { "cast(DoubleProp,'Edm.Double') eq 23" },
                    new [] { "cast(TimeSpanProp,'Edm.Duration') eq duration'PT23H'" },
                    new [] { "cast(GuidProp,'Edm.Guid') eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E" },
                    new [] { "cast(NullableShortProp,'Edm.Int16') eq 23" },
                    new [] { "cast(IntProp,'Edm.Int32') eq 23" },
                    new [] { "cast(LongProp,'Edm.Int64') eq 23" },
                    new [] { "cast(FloatProp,'Edm.Single') eq 23" },
                    new [] { "cast(StringProp,'Edm.String') eq 'hello'" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CastToQuotedPrimitiveType))]
        public void CastToQuotedPrimitiveType_Succeeds(string filterString)
        {
            //arrange
            var model = new DataTypes
            {
                BoolProp = true,
                DateTimeOffsetProp = DateTimeOffset.Parse("2001-01-01T12:00:00.000+08:00"),
                DecimalProp = 23,
                DoubleProp = 23,
                GuidProp = Guid.Parse("0EFDAECF-A9F0-42F3-A384-1295917AF95E"),
                NullableShortProp = 23,
                IntProp = 23,
                LongProp = 23,
                FloatProp = 23,
                StringProp = "hello",
                TimeSpanProp = TimeSpan.FromHours(23),
            };

            //act
            var filter = GetFilter<DataTypes>(filterString);
            bool result = RunFilter(filter, model);

            //assert
            Assert.True(result);
        }

        public static List<object[]> CastToUnquotedComplexType
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "cast(ExpressionBuilder.Tests.Data.Address) eq null" },
                    new [] { "cast(null, ExpressionBuilder.Tests.Data.Address) eq null" },
                    new [] { "cast('', ExpressionBuilder.Tests.Data.Address) eq null" },
                    new [] { "cast(SupplierAddress, ExpressionBuilder.Tests.Data.Address) eq null" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CastToUnquotedComplexType))]
        public void CastToUnquotedComplexType_ThrowsODataException(string filterString)
        {
            //arrange
            var expectedErrorMessage =
                "Encountered invalid type cast. " +
                "'ExpressionBuilder.Tests.Data.Address' is not assignable from 'ExpressionBuilder.Tests.Data.Product'.";

            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                expectedErrorMessage,
                exception.Message
            );
        }

        public static List<object[]> CastToQuotedComplexType
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "cast('ExpressionBuilder.Tests.Data.Address') eq null" },
                    new [] { "cast(null, 'ExpressionBuilder.Tests.Data.Address') eq null" },
                    new [] { "cast('', 'ExpressionBuilder.Tests.Data.Address') eq null" },
                    new [] { "cast(SupplierAddress, 'ExpressionBuilder.Tests.Data.Address') ne null" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CastToQuotedComplexType))]
        public void CastToQuotedComplexType_Succeeds(string filterString)
        {
            //arrange
            var model = new Product
            {
                SupplierAddress = new Address { City = "Redmond", },
            };

            //act
            var filter = GetFilter<Product>(filterString);
            bool result = RunFilter(filter, model);

            //assert
            Assert.True(result);
        }

        public static List<object[]> CastToUnquotedEntityType
        {
            get
            {
                return new List<object[]>
                {
                    new [] {
                        "cast(ExpressionBuilder.Tests.Data.DerivedProduct)/DerivedProductName eq null",
                        "Cast or IsOf Function must have a type in its arguments."
                    },
                    new [] {
                        "cast(null, ExpressionBuilder.Tests.Data.DerivedCategory)/DerivedCategoryName eq null",
                        "Encountered invalid type cast. " +
                        "'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'."
                    },
                    new [] {
                        "cast(Category, ExpressionBuilder.Tests.Data.DerivedCategory)/DerivedCategoryName eq null",
                        "Encountered invalid type cast. " +
                        "'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'."
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CastToUnquotedEntityType))]
        public void CastToUnquotedEntityType_ThrowsODataException(string filterString, string expectedMessage)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                expectedMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData("cast('ExpressionBuilder.Tests.Data.DerivedProduct')/DerivedProductName eq null", "$it => (($it As DerivedProduct).DerivedProductName == null)")]
        [InlineData("cast(Category,'ExpressionBuilder.Tests.Data.DerivedCategory')/DerivedCategoryName eq null", "$it => (($it.Category As DerivedCategory).DerivedCategoryName == null)")]
        public void CastToQuotedEntityOrComplexType_DerivedProductName(string filterString, string expectedExpression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }
        #endregion cast in query option

        #region 'isof' in query option
        [Theory]
        [InlineData("isof(Edm.Int16)", "$it => IIF(($it Is System.Int16), True, False)")]
        [InlineData("isof('ExpressionBuilder.Tests.Data.Product')", "$it => IIF(($it Is ExpressionBuilder.Tests.Data.Product), True, False)")]
        [InlineData("isof(ProductName,Edm.String)", "$it => IIF(($it.ProductName Is System.String), True, False)")]
        [InlineData("isof(Category,'ExpressionBuilder.Tests.Data.Category')", "$it => IIF(($it.Category Is ExpressionBuilder.Tests.Data.Category), True, False)")]
        [InlineData("isof(Category,'ExpressionBuilder.Tests.Data.DerivedCategory')", "$it => IIF(($it.Category Is ExpressionBuilder.Tests.Data.DerivedCategory), True, False)")]
        [InlineData("isof(Ranking, 'ExpressionBuilder.Tests.Data.SimpleEnum')", "$it => IIF(($it.Ranking Is ExpressionBuilder.Tests.Data.SimpleEnum), True, False)")]
        public void IsofMethod_Succeeds(string filterString, string expectedExpression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }

        [Theory]
        [InlineData("isof(null)")]
        [InlineData("isof(ProductName,null)")]
        public void Isof_WithNullTypeName_ThrowsArgumentNullException(string filterString)
        {
            //assert
            var exception = Assert.Throws<ArgumentNullException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                "Value cannot be null. (Parameter 'qualifiedName')",
                exception.Message
            );
        }

        [Theory]
        [InlineData("isof(NoSuchProperty,Edm.Int32)",
            "Could not find a property named 'NoSuchProperty' on type 'ExpressionBuilder.Tests.Data.DataTypes'.")]
        public void IsOfUndefinedSource_ThrowsODataException(string filterString, string errorMessage)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                errorMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData("isof(null,Edm.Binary)")]
        [InlineData("isof(null,Edm.Boolean)")]
        [InlineData("isof(null,Edm.Byte)")]
        [InlineData("isof(null,Edm.DateTimeOffset)")]
        [InlineData("isof(null,Edm.Decimal)")]
        [InlineData("isof(null,Edm.Double)")]
        [InlineData("isof(null,Edm.Duration)")]
        [InlineData("isof(null,Edm.Guid)")]
        [InlineData("isof(null,Edm.Int16)")]
        [InlineData("isof(null,Edm.Int32)")]
        [InlineData("isof(null,Edm.Int64)")]
        [InlineData("isof(null,Edm.SByte)")]
        [InlineData("isof(null,Edm.Single)")]
        [InlineData("isof(null,Edm.Stream)")]
        [InlineData("isof(null,Edm.String)")]
        [InlineData("isof(null,ExpressionBuilder.Tests.Data.SimpleEnum)")]
        [InlineData("isof(null,ExpressionBuilder.Tests.Data.FlagsEnum)")]

        [InlineData("isof(ByteArrayProp,Edm.Binary)")] // ByteArrayProp == null
        [InlineData("isof(IntProp,ExpressionBuilder.Tests.Data.SimpleEnum)")]
        [InlineData("isof(NullableShortProp,'Edm.Int16')")] // NullableShortProp == null

        [InlineData("isof('Edm.Binary')")]
        [InlineData("isof('Edm.Boolean')")]
        [InlineData("isof('Edm.Byte')")]
        [InlineData("isof('Edm.DateTimeOffset')")]
        [InlineData("isof('Edm.Decimal')")]
        [InlineData("isof('Edm.Double')")]
        [InlineData("isof('Edm.Duration')")]
        [InlineData("isof('Edm.Guid')")]
        [InlineData("isof('Edm.Int16')")]
        [InlineData("isof('Edm.Int32')")]
        [InlineData("isof('Edm.Int64')")]
        [InlineData("isof('Edm.SByte')")]
        [InlineData("isof('Edm.Single')")]
        [InlineData("isof('Edm.Stream')")]
        [InlineData("isof('Edm.String')")]
        [InlineData("isof('ExpressionBuilder.Tests.Data.SimpleEnum')")]
        [InlineData("isof('ExpressionBuilder.Tests.Data.FlagsEnum')")]

        [InlineData("isof(23,'Edm.Byte')")]
        [InlineData("isof(23,'Edm.Decimal')")]
        [InlineData("isof(23,'Edm.Double')")]
        [InlineData("isof(23,'Edm.Int16')")]
        [InlineData("isof(23,'Edm.Int64')")]
        [InlineData("isof(23,'Edm.SByte')")]
        [InlineData("isof(23,'Edm.Single')")]
        [InlineData("isof('hello','Edm.Stream')")]
        [InlineData("isof(0,'ExpressionBuilder.Tests.Data.FlagsEnum')")]
        [InlineData("isof(0,'ExpressionBuilder.Tests.Data.SimpleEnum')")]

        [InlineData("isof('2001-01-01T12:00:00.000+08:00','Edm.DateTimeOffset')")] // source is string
        [InlineData("isof('00000000-0000-0000-0000-000000000000','Edm.Guid')")] // source is string
        [InlineData("isof('23','Edm.Byte')")]
        [InlineData("isof('23','Edm.Int16')")]
        [InlineData("isof('23','Edm.Int32')")]
        [InlineData("isof('false','Edm.Boolean')")]
        [InlineData("isof('OData','Edm.Binary')")]
        [InlineData("isof('PT12H','Edm.Duration')")]
        [InlineData("isof(23,'Edm.String')")]
        [InlineData("isof('0','ExpressionBuilder.Tests.Data.FlagsEnum')")]
        [InlineData("isof('0','ExpressionBuilder.Tests.Data.SimpleEnum')")]
        public void IsOfPrimitiveType_Succeeds_WithFalse(string filterString)
        {
            //arrange
            var model = new DataTypes();

            //act
            var filter = GetFilter<DataTypes>(filterString);
            bool result = RunFilter(filter, model);

            //assert
            Assert.False(result);
        }

        public static List<object[]> IsOfUndefinedTarget
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "isof(Edm.DateTime)", "Edm.DateTime" },
                    new [] { "isof(Edm.Unknown)", "Edm.Unknown" },
                    new [] { "isof(null,Edm.DateTime)", "Edm.DateTime" },
                    new [] { "isof(null,Edm.Unknown)", "Edm.Unknown" },
                    new [] { "isof('2001-01-01T12:00:00.000',Edm.DateTime)", "Edm.DateTime" },
                    new [] { "isof('',Edm.Unknown)", "Edm.Unknown" },
                    new [] { "isof(DateTimeProp,Edm.DateTime)", "Edm.DateTime" },
                    new [] { "isof(IntProp,Edm.Unknown)", "Edm.Unknown" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsOfUndefinedTarget))]
        public void IsOfUndefinedTarget_ThrowsODataException(string filterString, string typeName)
        {
            // Arrange
            var errorMessage = string.Format(
                "The child type '{0}' in a cast was not an entity type. Casts can only be performed on entity types.",
                typeName);

            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                errorMessage,
                exception.Message
            );
        }

        public static List<object[]> IsOfQuotedUndefinedTarget
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "isof('Edm.DateTime')" },
                    new [] { "isof('Edm.Unknown')" },
                    new [] { "isof(null,'Edm.DateTime')" },
                    new [] { "isof(null,'Edm.Unknown')" },
                    new [] { "isof('2001-01-01T12:00:00.000','Edm.DateTime')" },
                    new [] { "isof('','Edm.Unknown')" },
                    new [] { "isof(DateTimeProp,'Edm.DateTime')" },
                    new [] { "isof(IntProp,'Edm.Unknown')" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsOfQuotedUndefinedTarget))]
        public void IsOfQuotedUndefinedTarget_ThrowsODataException(string filterString)
        {
            //arrange
            var expectedMessage = "Cast or IsOf Function must have a type in its arguments.";

            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));
            Assert.Equal
            (
                expectedMessage,
                exception.Message
            );
        }

        public static List<object[]> IsOfUnquotedComplexType
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "isof(ExpressionBuilder.Tests.Data.Address)" },
                    new [] { "isof(null,ExpressionBuilder.Tests.Data.Address)" },
                    new [] { "isof(null, ExpressionBuilder.Tests.Data.Address)" },
                    new [] { "isof(SupplierAddress,ExpressionBuilder.Tests.Data.Address)" },
                    new [] { "isof(SupplierAddress, ExpressionBuilder.Tests.Data.Address)" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsOfUnquotedComplexType))]
        public void IsOfUnquotedComplexType_ThrowsODataException(string filterString)
        {
            //arrange
            var expectedMessage =
                "Encountered invalid type cast. " +
                "'ExpressionBuilder.Tests.Data.Address' is not assignable from 'ExpressionBuilder.Tests.Data.Product'.";

            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                expectedMessage,
                exception.Message
            );
        }

        public static List<object[]> IsOfUnquotedEntityType
        {
            get
            {
                return new List<object[]>
                {
                    new [] {
                        "isof(ExpressionBuilder.Tests.Data.DerivedProduct)",
                        "Cast or IsOf Function must have a type in its arguments."
                    },
                    new [] {
                        "isof(null,ExpressionBuilder.Tests.Data.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'."
                    },
                    new [] {
                        "isof(null, ExpressionBuilder.Tests.Data.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'."
                    },
                    new [] {
                        "isof(Category,ExpressionBuilder.Tests.Data.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'."
                    },
                    new [] {
                        "isof(Category, ExpressionBuilder.Tests.Data.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'ExpressionBuilder.Tests.Data.DerivedCategory' is not assignable from 'ExpressionBuilder.Tests.Data.Product'."
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsOfUnquotedEntityType))]
        public void IsOfUnquotedEntityType_ThrowsODataException(string filterString, string expectedMessage)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<Product>(filterString));
            Assert.Equal
            (
                expectedMessage,
                exception.Message
            );
        }

        public static List<object[]> IsOfQuotedNonPrimitiveType
        {
            get
            {
                return new List<object[]>
                {
                    new [] { "isof('ExpressionBuilder.Tests.Data.DerivedProduct')" },
                    new [] { "isof(SupplierAddress,'ExpressionBuilder.Tests.Data.Address')" },
                    new [] { "isof(SupplierAddress, 'ExpressionBuilder.Tests.Data.Address')" },
                    new [] { "isof(Category,'ExpressionBuilder.Tests.Data.DerivedCategory')" },
                    new [] { "isof(Category, 'ExpressionBuilder.Tests.Data.DerivedCategory')" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsOfQuotedNonPrimitiveType))]
        public void IsOfQuotedNonPrimitiveType_Succeeds(string filterString)
        {
            //arrange
            var model = new DerivedProduct
            {
                SupplierAddress = new Address { City = "Redmond", },
                Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
            };

            //act
            var filter = GetFilter<Product>(filterString);
            bool result = RunFilter(filter, model);

            //assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("isof(null,'ExpressionBuilder.Tests.Data.Address')")]
        [InlineData("isof(null, 'ExpressionBuilder.Tests.Data.Address')")]
        [InlineData("isof(null,'ExpressionBuilder.Tests.Data.DerivedCategory')")]
        [InlineData("isof(null, 'ExpressionBuilder.Tests.Data.DerivedCategory')")]
        public void IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalse(string filterString)
        {
            //arrange
            var model = new DerivedProduct
            {
                SupplierAddress = new Address { City = "Redmond", },
                Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
            };

            //act
            var filter = GetFilter<Product>(filterString);
            bool result = RunFilter(filter, model);

            //assert
            Assert.False(result);
        }
        #endregion 'isof' in query option

        #region parameter alias for filter query option
        [Theory]
        // Parameter alias value is not null.
        [InlineData("IntProp eq @p", "1", "$it => ($it.IntProp == 1)")]
        [InlineData("BoolProp eq @p", "true", "$it => ($it.BoolProp == True)")]
        [InlineData("LongProp eq @p", "-123", "$it => ($it.LongProp == Convert(-123))")]
        [InlineData("FloatProp eq @p", "1.23", "$it => ($it.FloatProp == 1.23)")]
        [InlineData("DoubleProp eq @p", "4.56", "$it => ($it.DoubleProp == Convert(4.56))")]
        [InlineData("StringProp eq @p", "'abc'", "$it => ($it.StringProp == \"abc\")")]
        [InlineData("DateTimeOffsetProp eq @p", "2001-01-01T12:00:00.000+08:00", "$it => ($it.DateTimeOffsetProp == 01/01/2001 12:00:00 +08:00)")]
        [InlineData("TimeSpanProp eq @p", "duration'P8DT23H59M59.9999S'", "$it => ($it.TimeSpanProp == 8.23:59:59.9999000)")]
        [InlineData("GuidProp eq @p", "00000000-0000-0000-0000-000000000000", "$it => ($it.GuidProp == 00000000-0000-0000-0000-000000000000)")]
        [InlineData("SimpleEnumProp eq @p", "ExpressionBuilder.Tests.Data.SimpleEnum'First'", "$it => ($it.SimpleEnumProp == First)")]
        // Parameter alias value is null.
        [InlineData("NullableIntProp eq @p", "null", "$it => ($it.NullableIntProp == null)")]
        [InlineData("NullableBoolProp eq @p", "null", "$it => ($it.NullableBoolProp == null)")]
        [InlineData("NullableLongProp eq @p", "null", "$it => ($it.NullableLongProp == null)")]
        [InlineData("NullableSingleProp eq @p", "null", "$it => ($it.NullableSingleProp == null)")]
        [InlineData("NullableDoubleProp eq @p", "null", "$it => ($it.NullableDoubleProp == null)")]
        [InlineData("StringProp eq @p", "null", "$it => ($it.StringProp == null)")]
        [InlineData("NullableDateTimeOffsetProp eq @p", "null", "$it => ($it.NullableDateTimeOffsetProp == null)")]
        [InlineData("NullableTimeSpanProp eq @p", "null", "$it => ($it.NullableTimeSpanProp == null)")]
        [InlineData("NullableGuidProp eq @p", "null", "$it => ($it.NullableGuidProp == null)")]
        [InlineData("NullableSimpleEnumProp eq @p", "null", "$it => ($it.NullableSimpleEnumProp == null)")]
        // Parameter alias value is property.
        [InlineData("@p eq 1", "IntProp", "$it => ($it.IntProp == 1)")]
        [InlineData("@p eq true", "NullableBoolProp", "$it => ($it.NullableBoolProp == Convert(True))")]
        [InlineData("@p eq -123", "LongProp", "$it => ($it.LongProp == -123)")]
        [InlineData("@p eq 1.23", "FloatProp", "$it => ($it.FloatProp == 1.23)")]
        [InlineData("@p eq 4.56", "NullableDoubleProp", "$it => ($it.NullableDoubleProp == Convert(4.56))")]
        [InlineData("@p eq 'abc'", "StringProp", "$it => ($it.StringProp == \"abc\")")]
        [InlineData("@p eq 2001-01-01T12:00:00.000+08:00", "DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == 01/01/2001 12:00:00 +08:00)")]
        [InlineData("@p eq duration'P8DT23H59M59.9999S'", "TimeSpanProp", "$it => ($it.TimeSpanProp == 8.23:59:59.9999000)")]
        [InlineData("@p eq 00000000-0000-0000-0000-000000000000", "GuidProp", "$it => ($it.GuidProp == 00000000-0000-0000-0000-000000000000)")]
        [InlineData("@p eq ExpressionBuilder.Tests.Data.SimpleEnum'First'", "SimpleEnumProp", "$it => ($it.SimpleEnumProp == First)")]
        // Parameter alias value has built-in functions.
        [InlineData("@p eq 'abc'", "substring(StringProp,5)", "$it => ($it.StringProp.Substring(5) == \"abc\")")]
        [InlineData("2 eq @p", "IntProp add 1", "$it => (2 == ($it.IntProp + 1))")]
        [InlineData("EntityProp/AlternateAddresses/all(a: a/City ne @p)", "'abc'", "$it => $it.EntityProp.AlternateAddresses.All(a => (a.City != \"abc\"))")]
        public void ParameterAlias_Succeeds(string filterString, string parameterAliasValue, string expectedExpression)
        {
            //act
            var filter = GetFilter<DataTypes>
            (
                new Dictionary<string, string>
                {
                    ["$filter"] = filterString,
                    ["@p"] = parameterAliasValue
                },
                true
            );

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }

        [Theory]
        [InlineData("NullableIntProp eq @p", "$it => ($it.NullableIntProp == null)")]
        [InlineData("NullableBoolProp eq @p", "$it => ($it.NullableBoolProp == null)")]
        [InlineData("NullableDoubleProp eq @p", "$it => ($it.NullableDoubleProp == null)")]
        [InlineData("StringProp eq @p", "$it => ($it.StringProp == null)")]
        [InlineData("NullableDateTimeOffsetProp eq @p", "$it => ($it.NullableDateTimeOffsetProp == null)")]
        [InlineData("NullableSimpleEnumProp eq @p", "$it => ($it.NullableSimpleEnumProp == null)")]
        [InlineData("EntityProp/AlternateAddresses/any(a: a/City eq @p)", "$it => $it.EntityProp.AlternateAddresses.Any(a => (a.City == null))")]
        public void ParameterAlias_AssumedToBeNull_ValueNotFound(string filterString, string expectedExpression)
        {
            //act
            var filter = GetFilter<DataTypes>
            (
                new Dictionary<string, string>
                {
                    ["$filter"] = filterString
                },
                true
            );

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }

        [Fact]
        public void ParameterAlias_NestedCase_Succeeds()
        {
            //act
            var filter = GetFilter<DataTypes>
            (
                new Dictionary<string, string>
                {
                    ["$filter"] = "IntProp eq @p1",
                    ["@p1"] = "@p2",
                    ["@p2"] = "123"
                },
                true
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.IntProp == 123)");
        }

        [Fact]
        public void ParameterAlias_Throws_NotStartWithAt()
        {
            //assert
            var exception = Assert.Throws<ODataException>
            (
                () => GetFilter<DataTypes>
                (
                    new Dictionary<string, string>
                    {
                        ["$filter"] = "IntProp eq #p",
                        ["#p"] = "123"
                    }
                )
            );

            Assert.Equal
            (
                "Syntax error: character '#' is not valid at position 11 in 'IntProp eq #p'.",
                exception.Message
            );
        }

        [Theory]
        [InlineData("ByteArrayProp eq binary'I6v/'", "$it => ($it.ByteArrayProp == System.Byte[])", true)]
        [InlineData("ByteArrayProp ne binary'I6v/'", "$it => ($it.ByteArrayProp != System.Byte[])", false)]
        [InlineData("binary'I6v/' eq binary'I6v/'", "$it => (System.Byte[] == System.Byte[])", true)]
        [InlineData("binary'I6v/' ne binary'I6v/'", "$it => (System.Byte[] != System.Byte[])", false)]
        [InlineData("ByteArrayPropWithNullValue ne binary'I6v/'", "$it => ($it.ByteArrayPropWithNullValue != System.Byte[])", true)]
        [InlineData("ByteArrayPropWithNullValue ne ByteArrayPropWithNullValue", "$it => ($it.ByteArrayPropWithNullValue != $it.ByteArrayPropWithNullValue)", false)]
        [InlineData("ByteArrayPropWithNullValue ne null", "$it => ($it.ByteArrayPropWithNullValue != null)", false)]
        [InlineData("ByteArrayPropWithNullValue eq null", "$it => ($it.ByteArrayPropWithNullValue == null)", true)]
        [InlineData("null ne ByteArrayPropWithNullValue", "$it => (null != $it.ByteArrayPropWithNullValue)", false)]
        [InlineData("null eq ByteArrayPropWithNullValue", "$it => (null == $it.ByteArrayPropWithNullValue)", true)]
        public void ByteArrayComparisons(string filterString, string expectedExpression, bool expected)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);
            bool result = RunFilter
            (
                filter,
                new DataTypes
                {
                    ByteArrayProp = new byte[] { 35, 171, 255 }
                }
            );

            //assert
            Assert.Equal(expected, result);
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }

        [Theory]
        [InlineData("binary'AP8Q' add binary'AP8Q'", "Add")]
        [InlineData("binary'AP8Q' sub binary'AP8Q'", "Subtract")]
        [InlineData("binary'AP8Q' mul binary'AP8Q'", "Multiply")]
        [InlineData("binary'AP8Q' div binary'AP8Q'", "Divide")]
        public void DisAllowed_ByteArrayOperations(string filterString, string op)
        {
            //assert
            var exception = Assert.Throws<ODataException>(() => GetFilter<DataTypes>(filterString));

            Assert.Equal
            (
                string.Format(CultureInfo.InvariantCulture, "A binary operator with incompatible types was detected. Found operand types 'Edm.Binary' and 'Edm.Binary' for operator kind '{0}'.", op),
                exception.Message
            );
        }

        [Theory]
        [InlineData("binary'AP8Q' ge binary'AP8Q'")]
        [InlineData("binary'AP8Q' le binary'AP8Q'")]
        [InlineData("binary'AP8Q' lt binary'AP8Q'")]
        [InlineData("binary'AP8Q' gt binary'AP8Q'")]
        public void DisAllowed_ByteArrayComparisons(string filterString)
        {
            //assert
            Assert.Throws<InvalidOperationException>(() => GetFilter<DataTypes>(filterString));
        }

        [Theory]
        [InlineData("NullableUShortProp eq 12", "$it => (Convert($it.NullableUShortProp.Value) == Convert(12))")]
        [InlineData("NullableULongProp eq 12L", "$it => (Convert($it.NullableULongProp.Value) == Convert(12))")]
        [InlineData("NullableUIntProp eq 12", "$it => (Convert($it.NullableUIntProp.Value) == Convert(12))")]
        [InlineData("NullableCharProp eq 'a'", "$it => ($it.NullableCharProp.Value.ToString() == \"a\")")]
        public void Nullable_NonstandardEdmPrimitives(string filterString, string expectedExpression)
        {
            //act
            var filter = GetFilter<DataTypes>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new DataTypes()));
        }

        [Theory]
        [InlineData("Category/Product/ProductID in (1)", "$it => System.Collections.Generic.List`1[System.Int32].Contains($it.Category.Product.ProductID)")]
        [InlineData("Category/Product/GuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7)", "$it => System.Collections.Generic.List`1[System.Guid].Contains($it.Category.Product.GuidProperty)")]
        [InlineData("Category/Product/NullableGuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7)", "$it => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains($it.Category.Product.NullableGuidProperty)")]
        public void InOnNavigation(string filterString, string expectedExpression)
        {
            //act
            var filter = GetFilter<Product>(filterString);

            //assert
            AssertFilterStringIsCorrect(filter, expectedExpression);
        }

        [Fact]
        public void MultipleConstants_Are_Parameterized()
        {
            //act
            var filter = GetFilter<Product>("ProductName eq '1' or ProductName eq '2' or ProductName eq '3' or ProductName eq '4'");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (((($it.ProductName == \"1\") OrElse ($it.ProductName == \"2\")) OrElse ($it.ProductName == \"3\")) OrElse ($it.ProductName == \"4\"))");
        }

        [Fact]
        public void Constants_Are_Not_Parameterized_IfDisabled()
        {
            //act
            var filter = GetFilter<Product>("ProductName eq '1'");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == \"1\")");
        }

        [Fact]
        public void CollectionConstants_Are_Parameterized()
        {
            //act
            var filter = GetFilter<Product>("ProductName in ('Prod1', 'Prod2')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.String].Contains($it.ProductName)");
        }

        [Fact]
        public void CollectionConstants_Are_Not_Parameterized_If_Disabled()
        {
            //act
            var filter = GetFilter<Product>("ProductName in ('Prod1', 'Prod2')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.String].Contains($it.ProductName)");
        }

        [Fact]
        public void CollectionConstants_OfEnums_Are_Not_Parameterized_If_Disabled()
        {
            //act
            var filter = GetFilter<DataTypes>("SimpleEnumProp in ('First', 'Second')");

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[ExpressionBuilder.Tests.Data.SimpleEnum].Contains($it.SimpleEnumProp)");
        }
        #endregion parameter alias for filter query option

        #region Negative Tests
        [Fact]
        public void TypeMismatchInComparison()
        {
            //assert
            Assert.Throws<ODataException>(() => GetFilter<Product>("length(123) eq 12"));
        }
        #endregion Negative Tests

        // Used by Custom Method binder tests - by reflection
        private string PadRightInstance(string str, int number)
        {
            return str.PadRight(number);
        }

        // Used by Custom Method binder tests - by reflection
        private static string PadRightStatic(string str, int number)
        {
            return str.PadRight(number);
        }

        private T? ToNullable<T>(object value) where T : struct
        {
            return value == null ? null : (T?)Convert.ChangeType(value, typeof(T));
        }

        private void AssertFilterStringIsCorrect(Expression expression, string expected)
        {
            string resultExpression = ExpressionStringBuilder.ToString(expression);
            Assert.True(expected == resultExpression, string.Format("Expected expression '{0}' but the deserializer produced '{1}'", expected, resultExpression));
        }

        private bool RunFilter<TModel>(Expression<Func<TModel, bool>> filter, TModel instance)
            => filter.Compile().Invoke(instance);

        private static Expression<Func<TElement, bool>> GetSelectNestedFilter<TModel, TElement>(string selectString) where TModel : class
            => GetNestedFilter<TModel, TElement, PathSelectItem>
               (
                    new Dictionary<string, string> { ["$select"] = selectString }
               );

        private static Expression<Func<TElement, bool>> GetExpandNestedFilter<TModel, TElement>(string selectString) where TModel : class
            => GetNestedFilter<TModel, TElement, ExpandedNavigationSelectItem>
               (
                    new Dictionary<string, string> { ["$expand"] = selectString }
               );

        private static Expression<Func<TElement, bool>> GetNestedFilter<TModel, TElement, TPath>(IDictionary<string, string> queryOptions)
            where TModel : class
            where TPath : SelectItem
        {
            var selectAndExpand = ODataHelpers.GetSelectExpandClause<TModel>(queryOptions);

            var filterOption = selectAndExpand.SelectedItems                
                .OfType<TPath>()
                .Select(GetClause).First();

            return (Expression<Func<TElement, bool>>)filterOption.GetFilterExpression
            (
                typeof(TElement),
                ODataHelpers.GetODataQueryContext<TModel>()
            );

            static FilterClause GetClause(SelectItem item) => item switch
            {
                PathSelectItem select => select.FilterOption,
                ExpandedNavigationSelectItem expand => expand.FilterOption,
                _ => throw new NotSupportedException()
            };
        }

        private Expression<Func<TModel, bool>> GetFilter<TModel>(string filterString) where TModel : class
            => GetFilter<TModel>
            (
                new Dictionary<string, string> { ["$filter"] = filterString }
            );

        private Expression<Func<TModel, bool>> GetFilter<TModel>(IDictionary<string, string> queryOptions, bool useFilterOption = false) where TModel : class
           => GetFilterExpression<TModel>
           (
               GetFilterClause<TModel>(queryOptions, useFilterOption)
           );

        private FilterClause GetFilterClause<TModel>(IDictionary<string, string> queryOptions, bool useFilterOption = false) where TModel : class
            => ODataHelpers.GetFilterClause<TModel>(queryOptions, serviceProvider, useFilterOption);

        private Expression<Func<TModel, bool>> GetFilterExpression<TModel>(FilterClause filterClause) where TModel : class
            => (Expression<Func<TModel, bool>>)filterClause.GetFilterExpression(typeof(TModel), ODataHelpers.GetODataQueryContext<TModel>());
    }

    public static class StringExtender
    {
        public static string PadRightExStatic(this string str, int width)
        {
            return str.PadRight(width);
        }
    }
}
