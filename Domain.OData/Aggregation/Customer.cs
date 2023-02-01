using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.OData.Aggregation
{
    public class Customer
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public List<Sales> Sales { get; set; }
    }
}
