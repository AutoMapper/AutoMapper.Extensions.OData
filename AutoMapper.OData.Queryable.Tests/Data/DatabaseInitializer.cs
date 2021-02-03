using System;
using System.Collections.Generic;
using System.Linq;
using DAL.EFCore;

namespace AutoMapper.OData.Queryable.Tests.Data
{
    public static class DatabaseInitializer
    {
        public static void SeedDatabase(InMemoryObjectContext context)
        {

            var city1 = new TCity { Name = "London", Id = 1 };
            var city2 = new TCity { Name = "Leeds", Id = 2 };
            var builder1 = new TBuilder { Id = 1, Name = "Sam", CityId = 1, City = city1 };

            var mandator1 = new TMandator
            {
                Id = 1,
                Identity = Guid.NewGuid(),
                Name = "One",
                CreatedDate = new DateTime(2012, 12, 12)
            };

            mandator1.Buildings = new List<TBuilding>
            {
                new TBuilding
                {
                    Id = 1,
                    Identity = Guid.NewGuid(), LongName = "One L1",
                    BuilderId = builder1.Id,
                    Builder = builder1,
                    Mandator = mandator1,
                    MandatorId = mandator1.Id
                },
                new TBuilding
                {
                    Id = 2,
                    Identity = Guid.NewGuid(), LongName = "One L2",
                    BuilderId = builder1.Id,
                    Builder = builder1,
                    Mandator = mandator1,
                    MandatorId = mandator1.Id
                }
            };

            var mandator2 = new TMandator
            {
                Id = 2,
                Identity = Guid.NewGuid(),
                Name = "Two",
                CreatedDate = new DateTime(2012, 12, 12)
            };

            var builder2 = new TBuilder { Id = 2, Name = "John", CityId = 2, City = city1 };
            var builder3 = new TBuilder { Id = 3, Name = "Mark", CityId = 2, City = city2 };

            mandator2.Buildings = new List<TBuilding>
                {
                    new TBuilding
                    {
                        Id = 3,
                        Identity = Guid.NewGuid(), LongName = "Two L1",
                        BuilderId = builder2.Id,
                        Builder = builder2,
                        MandatorId = mandator2.Id,
                        Mandator = mandator2
                    },
                    new TBuilding
                    {
                        Id = 4,
                        Identity = Guid.NewGuid(), LongName = "Two L2",
                        BuilderId = builder3.Id,
                        Builder = builder3,
                        MandatorId = mandator2.Id,
                        Mandator = mandator2
                    },
                    new TBuilding
                    {
                        Id = 5,
                        Identity = Guid.NewGuid(), LongName = "Two L3",
                        BuilderId = builder3.Id,
                        Builder = builder3,
                        MandatorId = mandator2.Id,
                        Mandator = mandator2
                    }
                };

            context.City.Add(city1);
            context.City.Add(city2);

            context.Builder.Add(builder1);
            context.Builder.Add(builder2);
            context.Builder.Add(builder3);

            context.MandatorSet.Add(mandator1);
            context.MandatorSet.Add(mandator2);

            var buildings = context.BuildingSet;

            foreach (var building in context.MandatorSet.SelectMany(x => x.Buildings).Distinct())
                buildings.Add(building);
        }
    }
}
