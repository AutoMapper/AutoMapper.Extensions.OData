using System.Collections.Generic;

namespace Domain.OData.Aggregation
{
    public class Category
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<Product> Products { get; set; }
    }
}
