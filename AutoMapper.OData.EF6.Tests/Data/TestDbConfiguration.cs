using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AutoMapper.OData.EF6.Tests.Data
{
    public class TestDbConfiguration : DbConfiguration
    {
        public TestDbConfiguration()
        {
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("MSSQLLocalDB"));
        }
    }
}
