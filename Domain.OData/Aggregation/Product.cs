using System.Collections.Generic;

namespace Domain.OData.Aggregation
{
    public class Product
    {
        public string Id { get; set; }

        public Category Category { get; set; }

        public List<Sales> Sales { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public float TaxRate { get; set; }
    }
}
