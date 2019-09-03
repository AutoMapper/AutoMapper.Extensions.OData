using System.Data.Entity;

namespace DAL
{
    public class MyDbContext : DbContext
    {

        public MyDbContext()
            : base(DSN)
        {
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer<MyDbContext>(null);
        }

        public static System.String DSN { get; set; }

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
