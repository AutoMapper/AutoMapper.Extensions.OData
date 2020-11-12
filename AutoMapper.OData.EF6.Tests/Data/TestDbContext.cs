using DAL.EFCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace AutoMapper.OData.EF6.Tests.Data
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.SetInitializer(new DatabaseInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TMandator>()
                .HasMany(x => x.Buildings)
                .WithRequired(x => x.Mandator)
                .HasForeignKey(x => x.MandatorId);
        }

        public IDbSet<TMandator> MandatorSet { get; set; }

        public IDbSet<TBuilding> BuildingSet { get; set; }

        public IDbSet<TBuilder> Builder { get; set; }

        public IDbSet<TCity> City { get; set; }

    }
}
