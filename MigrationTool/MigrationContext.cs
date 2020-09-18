using DAL.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTool
{
    public class MigrationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQL2014;Database=YourDB;Trusted_Connection=True;");
        }

        public DbSet<TMandator> MandatorSet { get; set; }

        public DbSet<TBuilding> BuildingSet { get; set; }

        public DbSet<TBuilder> Builder { get; set; }

        public DbSet<TCity> City { get; set; }
    }
}
