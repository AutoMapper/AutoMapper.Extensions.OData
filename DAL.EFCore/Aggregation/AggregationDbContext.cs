using Microsoft.EntityFrameworkCore;

namespace DAL.EFCore.Aggregation
{
    public class AggregationDbContext : DbContext
    {
        public AggregationDbContext(DbContextOptions<AggregationDbContext> options) : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public DbSet<TblCategory> Categories { get; set; }

        public DbSet<TblCurrency> Currencies { get; set; }

        public DbSet<TblCustomer> Customers { get; set; }

        public DbSet<TblProduct> Products { get; set; }

        public DbSet<TblSales> Sales { get; set; }

        public DbSet<TblSalesOrganization> SalesOrganizations { get; set; }

        public DbSet<TblTime> Time { get; set; }
    }
}
