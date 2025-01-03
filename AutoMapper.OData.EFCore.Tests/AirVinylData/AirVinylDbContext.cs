using Microsoft.EntityFrameworkCore;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class AirVinylDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<VinylRecord> VinylRecords { get; set; }
        public DbSet<RecordStore> RecordStores { get; set; }
        public DbSet<PressingDetail> PressingDetails { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<DynamicProperty> DynamicVinylRecordProperties { get; set; }
        public DbSet<DoorManufacturer> DoorManufacturers { get; set; }

        public AirVinylDbContext(DbContextOptions<AirVinylDbContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DynamicProperty>().HasKey(d => new { d.Key, d.VinylRecordId });

            modelBuilder.Entity<Person>().Property(p => p.AmountOfCashToSpend).HasColumnType("decimal(8,2)");
            modelBuilder.Entity<Person>().OwnsMany(p => p.Cars, pc =>
            {
                pc.WithOwner().HasForeignKey(c => c.PersonId);
                pc.Property<int>("Id");
                pc.HasKey("Id");
            });

            // address is an owned type (= type without id)
            modelBuilder.Entity<RecordStore>().OwnsOne(p => p.StoreAddress, ra =>
            {
                ra.OwnsMany(a => a.Doors, ad =>
                {
                    ad.WithOwner().HasForeignKey(d => d.AddressId);
                    ad.Property<int>("Id");
                    ad.HasKey("Id");
                });
            });

            modelBuilder.Entity<SpecializedRecordStore>().OwnsOne(p => p.StoreAddress, ra =>
            {
                ra.OwnsMany(a => a.Doors, ad =>
                {
                    ad.WithOwner().HasForeignKey(d => d.AddressId);
                    ad.Property<int>("Id");
                    ad.HasKey("Id");
                });
            });
        }
    }
}
