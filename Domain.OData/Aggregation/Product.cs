using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.OData.Aggregation
{
    public class Product
    {
        [Key]
        public string Id { get; set; }

        public Category Category { get; set; }

        public List<Sales> Sales { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public decimal TaxRate { get; set; }
    }
}
