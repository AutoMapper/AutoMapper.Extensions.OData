using System.Collections.Generic;

namespace Domain.OData.Aggregation
{
    public class Customer
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public List<Sales> Sales { get; set; }
    }
}
