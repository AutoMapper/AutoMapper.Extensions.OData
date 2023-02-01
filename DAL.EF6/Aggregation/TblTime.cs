using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EF6.Aggregation
{
    [Table("TblTime")]
    public class TblTime
    {
        [Key]
        public string FldDate { get; set; }

        [Required]
        public string FldMonth { get; set; }

        [Required]
        public string FldQuarter { get; set; }

        [Required]
        public int FldYear { get; set; }
    }
}
