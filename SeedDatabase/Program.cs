using DAL.EFCore;
using DAL.EFCore.Aggregation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SeedDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddDbContext<MyDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient)
                .AddDbContext<AggregationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient)
                .BuildServiceProvider();

            SeedMyDbContext(serviceProvider);
            SeedAggregationDbContext(serviceProvider);
        }

        private static void SeedMyDbContext(IServiceProvider serviceProvider)
        {
            using MyDbContext context = serviceProvider.GetRequiredService<MyDbContext>();
            
            if (context.City.Any())
                return;

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
                CreatedDate = new DateTime(2012, 12, 12),
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
                CreatedDate = new DateTime(2012, 12, 12),
                Buildings = new List<TBuilding>
                    {
                        new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L1", BuilderId = builders.First(b => b.Name == "John").Id  },
                        new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L2", BuilderId = builders.First(b => b.Name == "Mark").Id  },
                        new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L3", BuilderId = builders.First(b => b.Name == "Mark").Id  }
                    }
            });
            context.SaveChanges();
        }

        private static void SeedAggregationDbContext(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<AggregationDbContext>();

            if (context.Sales.Any())
            {
                return;
            }

            // As described in OData Extension for "Data Aggregation Version 4.0/2.3 Example Data":
            // https://docs.oasis-open.org/odata/odata-data-aggregation-ext/v4.0/cs02/odata-data-aggregation-ext-v4.0-cs02.html#_Toc435016565

            context.Categories.AddRange(new[]
            {
                new TblCategory { FldId = "PG1", FldName= "Food" },
                new TblCategory { FldId = "PG2", FldName= "Non-Food" }
            });

            context.Products.AddRange(new[]
            {
                new TblProduct { FldId ="P1", FldCategoryId = "PG1", FldName = "Sugar", FldColor ="White", FldTaxRate = 0.06m },
                new TblProduct { FldId ="P2", FldCategoryId = "PG1", FldName = "Coffee", FldColor ="Brown", FldTaxRate = 0.06m },
                new TblProduct { FldId ="P3", FldCategoryId = "PG2", FldName = "Paper", FldColor ="White", FldTaxRate = 0.14m },
                new TblProduct { FldId ="P4", FldCategoryId = "PG2", FldName = "Pencil", FldColor ="Black", FldTaxRate = 0.14m }
            });

            context.Time.AddRange(new[]
            {
                new TblTime { FldDate = "2012-01-01", FldMonth = "2012-01", FldQuarter = "2012-1", FldYear = 2012 },
                new TblTime { FldDate = "2012-04-01", FldMonth = "2012-04", FldQuarter = "2012-2", FldYear = 2012 },
                new TblTime { FldDate = "2012-04-10", FldMonth = "2012-04", FldQuarter = "2012-2", FldYear = 2012 },
                new TblTime { FldDate = "2012-01-03", FldMonth = "2012-01", FldQuarter = "2012-1", FldYear = 2012 },
                new TblTime { FldDate = "2012-08-07", FldMonth = "2012-08", FldQuarter = "2012-3", FldYear = 2012 },
                new TblTime { FldDate = "2012-11-09", FldMonth = "2012-11", FldQuarter = "2012-4", FldYear = 2012 },
                new TblTime { FldDate = "2012-08-06", FldMonth = "2012-08", FldQuarter = "2012-3", FldYear = 2012 },
                new TblTime { FldDate = "2012-11-22", FldMonth = "2012-11", FldQuarter = "2012-4", FldYear = 2012 }
            });

            context.SalesOrganizations.Add(new TblSalesOrganization { FldId = "Sales", FldName = "Corpate Sales" });
            context.SalesOrganizations.AddRange(new[]
            { 
                new TblSalesOrganization { FldId = "US", FldSuperordinateId = "Sales", FldName = "US" },
                new TblSalesOrganization { FldId = "EMEA", FldSuperordinateId = "Sales", FldName = "EMEA" }
            });
            context.SalesOrganizations.AddRange(new[]
            {
                new TblSalesOrganization { FldId = "US East", FldSuperordinateId = "US", FldName = "US East" },
                new TblSalesOrganization { FldId = "US West", FldSuperordinateId = "US", FldName = "US West" },
                new TblSalesOrganization { FldId = "EMEA Central", FldSuperordinateId = "EMEA", FldName = "EMEA Central" }
            });

            context.Customers.AddRange(new[]
            {
                new TblCustomer { FldId = "C1", FldName = "Joe", FldCountry = "USA" },
                new TblCustomer { FldId = "C2", FldName = "Sue", FldCountry = "USA" },
                new TblCustomer { FldId = "C3", FldName = "Sue", FldCountry = "Netherlands" },
                new TblCustomer { FldId = "C4", FldName = "Luc", FldCountry = "France" }
            });

            context.Currencies.AddRange(new[]
            {
                new TblCurrency { FldCode = "USD", FldName = "US Dollar" },
                new TblCurrency { FldCode = "EUR", FldName = "Euro" }
            });

            context.Sales.AddRange(new[]
            { 
                new TblSales { FldCustomerId = "C1", FldTimeId = "2012-01-03", FldProductId = "P3", FldSalesOrganizationId = "US West", FldCurrencyCode = "USD", FldAmount = 1},
                new TblSales { FldCustomerId = "C1", FldTimeId = "2012-04-10", FldProductId = "P1", FldSalesOrganizationId = "US West", FldCurrencyCode = "USD", FldAmount = 2},
                new TblSales { FldCustomerId = "C1", FldTimeId = "2012-08-07", FldProductId = "P2", FldSalesOrganizationId = "US West", FldCurrencyCode = "USD", FldAmount = 4},
                new TblSales { FldCustomerId = "C2", FldTimeId = "2012-01-03", FldProductId = "P2", FldSalesOrganizationId = "US East", FldCurrencyCode = "USD", FldAmount = 8},
                new TblSales { FldCustomerId = "C2", FldTimeId = "2012-11-09", FldProductId = "P3", FldSalesOrganizationId = "US East", FldCurrencyCode = "USD", FldAmount = 4},
                new TblSales { FldCustomerId = "C3", FldTimeId = "2012-04-01", FldProductId = "P1", FldSalesOrganizationId = "EMEA Central", FldCurrencyCode = "EUR", FldAmount = 2},
                new TblSales { FldCustomerId = "C3", FldTimeId = "2012-08-06", FldProductId = "P3", FldSalesOrganizationId = "EMEA Central", FldCurrencyCode = "EUR", FldAmount = 1},
                new TblSales { FldCustomerId = "C3", FldTimeId = "2012-11-22", FldProductId = "P3", FldSalesOrganizationId = "EMEA Central", FldCurrencyCode = "EUR", FldAmount = 2}
            });

            context.SaveChanges();
        }
    }
}
