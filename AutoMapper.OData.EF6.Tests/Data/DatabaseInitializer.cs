using DAL.EF6;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace AutoMapper.OData.EF6.Tests.Data
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestDbContext>
    {
        protected override void Seed(TestDbContext context)
        {
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
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "One L1", BuilderId = builders.First(b => b.Name == "Sam").Id, FloorAmount = 4 },
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "One L2", BuilderId = builders.First(b => b.Name == "Sam").Id, FloorAmount = 5 }
                }
            });
            context.MandatorSet.Add(new TMandator
            {
                Identity = Guid.NewGuid(),
                Name = "Two",
                CreatedDate = new DateTime(2012, 12, 12),
                Buildings = new List<TBuilding>
                {
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L1", BuilderId = builders.First(b => b.Name == "John").Id, FloorAmount = 1 },
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L2", BuilderId = builders.First(b => b.Name == "Mark").Id, FloorAmount = 2 },
                    new TBuilding { Identity =  Guid.NewGuid(), LongName = "Two L3", BuilderId = builders.First(b => b.Name == "Mark").Id, FloorAmount = 3 }
                }
            });
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
