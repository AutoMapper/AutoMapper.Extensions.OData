using DAL.EFCore;
using DAL.EFCore.Aggregation;
using Microsoft.EntityFrameworkCore;

namespace MigrationTool
{
    public class MigrationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQL2016;Database=YourDB;Trusted_Connection=True;");
        }

        public DbSet<TMandator> MandatorSet { get; set; }

        public DbSet<TBuilding> BuildingSet { get; set; }

        public DbSet<TBuilder> Builder { get; set; }

        public DbSet<TCity> City { get; set; }

        public DbSet<TblCategory> Categories { get; set; }

        public DbSet<TblCurrency> Currencies { get; set; }

        public DbSet<TblCustomer> Customers { get; set; }

        public DbSet<TblProduct> Products { get; set; }

        public DbSet<TblSales> Sales { get; set; }

        public DbSet<TblSalesOrganization> SalesOrganizations { get; set; }

        public DbSet<TblTime> Time { get; set; }
    }
}
