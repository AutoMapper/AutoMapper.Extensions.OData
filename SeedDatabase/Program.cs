using DAL.EFCore;
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

            IServiceProvider serviceProvider = new ServiceCollection().AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient)
                .BuildServiceProvider();

            using (MyDbContext context = serviceProvider.GetRequiredService<MyDbContext>())
            {
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
        }
    }
}
