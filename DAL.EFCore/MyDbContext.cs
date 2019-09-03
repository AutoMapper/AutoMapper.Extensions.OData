using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.EFCore
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public DbSet<TMandator> MandatorSet { get; set; }

        public DbSet<TBuilding> BuildingSet { get; set; }

        public DbSet<TBuilder> Builder { get; set; }

        public DbSet<TCity> City { get; set; }

    }
}
