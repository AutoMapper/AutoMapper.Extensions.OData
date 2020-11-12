using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;

namespace DAL.EF6
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(string connectionString)
            : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer<MyDbContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TMandator>().HasMany(x => x.Buildings).WithRequired(x => x.Mandator).HasForeignKey(x => x.MandatorId);
        }

        public IDbSet<TMandator> MandatorSet { get; set; }

        public IDbSet<TBuilding> BuildingSet { get; set; }

        public IDbSet<TBuilder> Builder { get; set; }

        public IDbSet<TCity> City { get; set; }

    }
}
