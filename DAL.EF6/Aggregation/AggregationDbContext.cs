using System.Data.Entity;

namespace DAL.EF6.Aggregation
{
    public class AggregationDbContext : DbContext
    {
        public AggregationDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer<MyDbContext>(null);
        }

        public IDbSet<TblCategory> Categories { get; set; }

        public IDbSet<TblCurrency> Currencies { get; set; }

        public IDbSet<TblCustomer> Customers { get; set; }

        public IDbSet<TblProduct> Products { get; set; }

        public IDbSet<TblSales> Sales { get; set; }

        public IDbSet<TblSalesOrganization> SalesOrganizations { get; set; }

        public IDbSet<TblTime> Time { get; set; }
    }
}
