namespace Domain.OData.Aggregation
{
    public class Sales
    {
        public int Id { get; set; }

        public Customer Customer { get; set; }

        public Time Time { get; set; }

        public Product Product { get; set; }

        public SalesOrganization SalesOrganization { get; set; }

        public Currency Currency { get; set; }

        public decimal Amount { get; set; }
    }
}
