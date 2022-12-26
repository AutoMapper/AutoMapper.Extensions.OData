using System.ComponentModel.DataAnnotations;

namespace Domain.OData.Aggregation
{
    public class Sales
    {
        [Key]
        public int Id { get; set; }

        public Customer Customer { get; set; }

        public Time Time { get; set; }

        public string ProductId { get; set; }

        public Product Product { get; set; }

        public SalesOrganization SalesOrganization { get; set; }

        public string CurrencyCode { get; set; }

        public Currency Currency { get; set; }

        public decimal Amount { get; set; }
    }
}
