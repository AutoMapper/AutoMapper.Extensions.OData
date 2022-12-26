using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.OData.Aggregation
{
    public class Category
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public List<Product> Products { get; set; }
    }
}
