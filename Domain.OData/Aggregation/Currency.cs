using System.ComponentModel.DataAnnotations;

namespace Domain.OData.Aggregation
{
    public class Currency
    {
        [Key]
        public string Code { get; set; }

        public string Name { get; set; }
    }
}
