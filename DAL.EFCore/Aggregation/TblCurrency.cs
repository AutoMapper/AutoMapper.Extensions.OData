using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore.Aggregation
{
    [Table("TblCurrency")]
    public class TblCurrency
    {
        [Key]
        public string FldCode { get; set; }

        [Required]
        public string FldName { get; set; }
    }
}
