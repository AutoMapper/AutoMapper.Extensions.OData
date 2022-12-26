using System.ComponentModel.DataAnnotations;

namespace Domain.OData.Aggregation
{
    public class Time
    {
        [Key]
        public string Date { get; set; }

        public string Month { get; set; }

        public string Quarter { get; set; }

        public int Year { get; set; }
    }
}
