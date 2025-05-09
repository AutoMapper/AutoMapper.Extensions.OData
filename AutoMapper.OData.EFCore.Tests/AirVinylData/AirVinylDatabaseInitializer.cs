using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public static class AirVinylDatabaseInitializer
    {
        public static void SeedDatabase(AirVinylDbContext context)
        {
            context.PressingDetails.AddRange(
                new PressingDetail()
                {
                    //PressingDetailId = 1,
                    Description = "Audiophile LP",
                    Grams = 180,
                    Inches = 12
                },
                new PressingDetail()
                {
                    //PressingDetailId = 2,
                    Description = "Regular LP",
                    Grams = 140,
                    Inches = 12
                },
                new PressingDetail()
                {
                    //PressingDetailId = 3,
                    Description = "Audiophile Single",
                    Grams = 50,
                    Inches = 7
                },
                new PressingDetail()
                {
                    //PressingDetailId = 4,
                    Description = "Regular Single",
                    Grams = 40,
                    Inches = 7
                });
            context.SaveChanges();
            

            context.People.AddRange(
                new Person()
                {
                    //PersonId = 1,
                    DateOfBirth = new DateTimeOffset(new DateTime(1981, 5, 5)),
                    Email = "kevin@kevindockx.com",
                    FirstName = "Kevin",
                    LastName = "Dockx",
                    Gender = Gender.Male,
                    NumberOfRecordsOnWishList = 10,
                    AmountOfCashToSpend = 300,
                    Cars =
                    [
                        new() { Name = "Toyota Camry" }, 
                        new() { Name = "Honda Accord" }
                    ]
                },
                new Person()
                {
                    //PersonId = 2,
                    DateOfBirth = new DateTimeOffset(new DateTime(1986, 3, 6)),
                    Email = "sven@someemailprovider.com",
                    FirstName = "Sven",
                    LastName = "Vercauteren",
                    Gender = Gender.Male,
                    NumberOfRecordsOnWishList = 34,
                    AmountOfCashToSpend = 2000,
                    Cars =
                    [
                        new() { Name = "Mercedes 200" },
                        new() { Name = "Honda Civic" }
                    ]
                },
                new Person()
                {
                    //PersonId = 3,
                    DateOfBirth = new DateTimeOffset(new DateTime(1977, 12, 27)),
                    Email = "nele@someemailprovider.com",
                    FirstName = "Nele",
                    LastName = "Verheyen",
                    Gender = Gender.Female,
                    NumberOfRecordsOnWishList = 120,
                    AmountOfCashToSpend = 100,
                    Cars =
                    [
                        new() { Name = "Toyota Rav4" },
                        new() { Name = "Honda Passport" }
                    ]
                },
                new Person()
                {
                    //PersonId = 4,
                    DateOfBirth = new DateTimeOffset(new DateTime(1983, 5, 18)),
                    Email = "nils@someemailprovider.com",
                    FirstName = "Nils",
                    LastName = "Missorten",
                    Gender = Gender.Male,
                    NumberOfRecordsOnWishList = 23,
                    AmountOfCashToSpend = 2500,
                    Cars =
                    [
                        new() { Name = "Jeep Wrangler" },
                        new() { Name = "Honda Pilot" }
                    ]
                },
                new Person()
                {
                    //PersonId = 5,
                    DateOfBirth = new DateTimeOffset(new DateTime(1981, 10, 15)),
                    Email = "tim@someemailprovider.com",
                    FirstName = "Tim",
                    LastName = "Van den Broeck",
                    Gender = Gender.Male,
                    NumberOfRecordsOnWishList = 19,
                    AmountOfCashToSpend = 90,
                    Cars =
                    [
                        new() { Name = "Toyota Corolla" }
                    ]
                },
                new Person()
                {
                    //PersonId = 6,
                    DateOfBirth = new DateTimeOffset(new DateTime(1981, 1, 16)),
                    Email = null,
                    FirstName = "Kenneth",
                    LastName = "Mills",
                    Gender = Gender.Male,
                    NumberOfRecordsOnWishList = 98,
                    AmountOfCashToSpend = 200,
                    Cars =
                    [
                        new() { Name = "Renault 19" }
                    ]
                }
            );
            context.SaveChanges();

            ICollection<PressingDetail> pressingDetails = [.. context.PressingDetails];
            ICollection<Person> people = [.. context.People];

            context.VinylRecords.AddRange(
                new VinylRecord()
                {
                    //VinylRecordId = 1,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Nirvana",
                    Title = "Nevermind",
                    CatalogNumber = "ABC/111",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile LP").PressingDetailId,//1,
                    Year = 1991
                },
                new VinylRecord()
                {
                    //VinylRecordId = 2,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Arctic Monkeys",
                    Title = "AM",
                    CatalogNumber = "EUI/111",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2,
                    Year = 2013
                },
                new VinylRecord()
                {
                    //VinylRecordId = 3,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Beatles",
                    Title = "The White Album",
                    CatalogNumber = "DEI/113",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2,
                    Year = 1968
                },
                new VinylRecord()
                {
                    //VinylRecordId = 4,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Beatles",
                    Title = "Sergeant Pepper's Lonely Hearts Club Band",
                    CatalogNumber = "DPI/123",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2,
                    Year = 1967
                },
                new VinylRecord()
                {
                    //VinylRecordId = 5,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Nirvana",
                    Title = "Bleach",
                    CatalogNumber = "DPI/123",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile LP").PressingDetailId,//1,
                    Year = 1989
                },
                new VinylRecord()
                {
                    //VinylRecordId = 6,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Leonard Cohen",
                    Title = "Suzanne",
                    CatalogNumber = "PPP/783",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile Single").PressingDetailId,//3,
                    Year = 1967
                },
                new VinylRecord()
                {
                    //VinylRecordId = 7,
                    PersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Artist = "Marvin Gaye",
                    Title = "What's Going On",
                    CatalogNumber = "MVG/445",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile LP").PressingDetailId,//1,
                    Year = null
                },
                new VinylRecord()
                {
                    //VinylRecordId = 8,
                    PersonId = people.Single(p => p.Email == "sven@someemailprovider.com").PersonId,//2,
                    Artist = "Nirvana",
                    Title = "Nevermind",
                    CatalogNumber = "ABC/111",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile LP").PressingDetailId,//1,
                    Year = 1991
                },
                new VinylRecord()
                {
                    //VinylRecordId = 9,
                    PersonId = people.Single(p => p.Email == "sven@someemailprovider.com").PersonId,//2,
                    Artist = "Cher",
                    Title = "Closer to the Truth",
                    CatalogNumber = "CHE/190",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2,
                    Year = 2013
                },
                new VinylRecord()
                {
                    //VinylRecordId = 10,
                    PersonId = people.Single(p => p.Email == "nele@someemailprovider.com").PersonId,//3,
                    Artist = "The Dandy Warhols",
                    Title = "Thirteen Tales From Urban Bohemia",
                    CatalogNumber = "TDW/516",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2
                },
                new VinylRecord()
                {
                    //VinylRecordId = 11,
                    PersonId = people.Single(p => p.Email == "nils@someemailprovider.com").PersonId,//4,
                    Artist = "Justin Bieber",
                    Title = "Baby",
                    CatalogNumber = "OOP/098",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile Single").PressingDetailId,//3
                },
                new VinylRecord()
                {
                    //VinylRecordId = 12,
                    PersonId = people.Single(p => p.Email == "nils@someemailprovider.com").PersonId,//4,
                    Artist = "The Prodigy",
                    Title = "Music for the Jilted Generation",
                    CatalogNumber = "NBE/864",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2
                },
                new VinylRecord()
                {
                    //VinylRecordId = 13,
                    PersonId = people.Single(p => p.Email == "tim@someemailprovider.com").PersonId,//5,
                    Artist = "Anne Clarke",
                    Title = "Our Darkness",
                    CatalogNumber = "TII/339",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Audiophile Single").PressingDetailId,//3
                },
                new VinylRecord()
                {
                    //VinylRecordId = 14,
                    PersonId = people.Single(p => p.Email == "tim@someemailprovider.com").PersonId,//5,
                    Artist = "Dead Kennedys",
                    Title = "Give Me Convenience or Give Me Death",
                    CatalogNumber = "DKE/864",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular LP").PressingDetailId,//2
                },
                new VinylRecord()
                {
                    //VinylRecordId = 15,
                    PersonId = people.Single(p => p.Email == "tim@someemailprovider.com").PersonId,//5,
                    Artist = "Sisters of Mercy",
                    Title = "Temple of Love",
                    CatalogNumber = "IIE/824",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular Single").PressingDetailId,//4
                },
                new VinylRecord()
                {
                    //VinylRecordId = 16,
                    PersonId = people.Single(p => p.Email == null).PersonId,//6,
                    Artist = "Abba",
                    Title = "Gimme Gimme Gimme",
                    CatalogNumber = "TDW/516",
                    PressingDetailId = pressingDetails.Single(p => p.Description == "Regular Single").PressingDetailId,//4
                }
            );
            context.SaveChanges();

            ICollection<VinylRecord> vinylRecords = [.. context.VinylRecords];

            context.DynamicVinylRecordProperties.Add(new DynamicProperty()
            {
                VinylRecordId = vinylRecords.First(r => r.Title == "Nevermind").VinylRecordId,//1
                Key = "Publisher",
                Value = "Geffen"
            });
            context.DynamicVinylRecordProperties.Add(new DynamicProperty()
            {
                VinylRecordId = vinylRecords.First(r => r.Title == "Nevermind").VinylRecordId,//1
                Key = "SomeData",
                Value = new { TestProp = "value" }
            });
            context.SaveChanges();

            context.DoorManufacturers.AddRange(
                new DoorManufacturer { Name = "Serta" }, 
                new DoorManufacturer { Name = "Sealy" },
                new DoorManufacturer { Name = "Ikea" },
                new DoorManufacturer { Name = "Hardwood" }
            );
            context.DoorKnobs.AddRange(
                new DoorKnob { Style = "Circular" },
                new DoorKnob { Style = "Lever" }
            );

            context.SaveChanges();

            ICollection<DoorManufacturer> doorManufacturers = [.. context.DoorManufacturers];
            ICollection<DoorKnob> doorKnobs = [.. context.DoorKnobs];

            context.RecordStores.AddRange(
                new SpecializedRecordStore()
                {
                    //RecordStoreId = 2,
                    Name = "Indie Records, Inc",
                    Tags = new List<string>() { "Rock", "Indie", "Alternative" },
                    Specialization = "Indie",
                    StoreAddress = new Address()
                    {
                        //RecordStoreId = 2,
                        City = "Antwerp",
                        PostalCode = "2000",
                        Street = "1, Main Street",
                        Country = "Belgium",
                        RoomNumbers = [3, 4, 5, 6],
                        Doors =
                        [
                            new() { Name = "Front Door", DoorManufacturerId = doorManufacturers.Single(m => m.Name == "Serta").Id, DoorKnobId = doorKnobs.Single(k => k.Style == "Circular").Id },
                            new() { Name = "Side Door", DoorManufacturerId = doorManufacturers.Single(m => m.Name == "Hardwood").Id, DoorKnobId = doorKnobs.Single(k => k.Style == "Lever").Id }
                        ]
                    }
                },
                new SpecializedRecordStore()
                {
                    //RecordStoreId = 3,
                    Name = "Rock Records, Inc",
                    Tags = new List<string>() { "Rock", "Pop" },
                    Specialization = "Rock",
                    StoreAddress = new Address()
                    {
                        //RecordStoreId = 3,
                        City = "Antwerp",
                        PostalCode = "2000",
                        Street = "5, Big Street",
                        Country = "Belgium",
                        RoomNumbers = [2, 3, 4, 5],
                        Doors =
                        [
                            new() { Name = "Main Door", DoorManufacturerId = doorManufacturers.Single(m => m.Name == "Serta").Id, DoorKnobId = doorKnobs.Single(k => k.Style == "Circular").Id },
                            new() { Name = "Cabinet Door", DoorManufacturerId = doorManufacturers.Single(m => m.Name == "Serta").Id, DoorKnobId = doorKnobs.Single(k => k.Style == "Circular").Id}
                        ]
                    }
                },
                new RecordStore()
                {
                    //RecordStoreId = 1,
                    Name = "All Your Music Needs",
                    Tags = new List<string>() { "Rock", "Pop", "Indie", "Alternative" },
                    StoreAddress = new Address()
                    {
                        //RecordStoreId = 1,
                        City = "Antwerp",
                        PostalCode = "2000",
                        Street = "25, Fluffy Road",
                        Country = "Belgium",
                        RoomNumbers = [1, 2, 3, 4],
                        Doors =
                        [
                            new() { Name = "Bedroom Door", DoorManufacturerId = doorManufacturers.Single(m => m.Name == "Serta").Id, DoorKnobId = doorKnobs.Single(k => k.Style == "Circular").Id },
                            new() { Name = "Balcony Door", DoorManufacturerId = doorManufacturers.Single(m => m.Name == "Sealy").Id, DoorKnobId = doorKnobs.Single(k => k.Style == "Lever").Id}
                        ]
                    }
                }
            );
            context.SaveChanges();

            ICollection<RecordStore> recordStores = [.. context.RecordStores];

            context.Ratings.AddRange(
                new Rating()
                {
                    //RatingId = 1,
                    RecordStoreId = recordStores.Single(s => s.Name == "All Your Music Needs").RecordStoreId,//1,
                    RatedByPersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Value = 4
                },
                new Rating()
                {
                    //RatingId = 2,
                    RecordStoreId = recordStores.Single(s => s.Name == "All Your Music Needs").RecordStoreId,//1,
                    RatedByPersonId = people.Single(p => p.Email == "sven@someemailprovider.com").PersonId,//2,
                    Value = 4
                },
                new Rating()
                {
                    //RatingId = 3,
                    RecordStoreId = recordStores.Single(s => s.Name == "All Your Music Needs").RecordStoreId,//1,
                    RatedByPersonId = people.Single(p => p.Email == "nele@someemailprovider.com").PersonId,//3,
                    Value = 4
                },
                new Rating()
                {
                    //RatingId = 4,
                    RecordStoreId = recordStores.Single(s => s.Name == "Indie Records, Inc").RecordStoreId,//2,
                    RatedByPersonId = people.Single(p => p.Email == "kevin@kevindockx.com").PersonId,//1,
                    Value = 5
                },
                new Rating()
                {
                    //RatingId = 5,
                    RecordStoreId = recordStores.Single(s => s.Name == "Indie Records, Inc").RecordStoreId,//2,
                    RatedByPersonId = people.Single(p => p.Email == "sven@someemailprovider.com").PersonId,//2,
                    Value = 4
                },
                new Rating()
                {
                    //RatingId = 6,
                    RecordStoreId = recordStores.Single(s => s.Name == "Rock Records, Inc").RecordStoreId,//3,
                    RatedByPersonId = people.Single(p => p.Email == "nele@someemailprovider.com").PersonId,//3,
                    Value = 5
                },
                new Rating()
                {
                    //RatingId = 7,
                    RecordStoreId = recordStores.Single(s => s.Name == "Rock Records, Inc").RecordStoreId,//3,
                    RatedByPersonId = people.Single(p => p.Email == "sven@someemailprovider.com").PersonId,//2,
                    Value = 4
                }
            );
            context.SaveChanges();
        }
    }
}
