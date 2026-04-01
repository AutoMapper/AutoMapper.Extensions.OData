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
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=WebApiEFCoreDatabase;ConnectRetryCount=0");
        }

        public DbSet<TMandator> MandatorSet { get; set; }

        public DbSet<TBuilding> BuildingSet { get; set; }

        public DbSet<TBuilder> Builder { get; set; }

        public DbSet<TCity> City { get; set; }
    }
}
