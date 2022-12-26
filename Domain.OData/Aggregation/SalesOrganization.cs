namespace Domain.OData.Aggregation
{
    public class SalesOrganization
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public  SalesOrganization Superordinate { get; set; }
    }
}
