using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore.Aggregation
{
    [Table("TblSalesOrganization")]
    public class TblSalesOrganization
    {
        [Key]
        public string FldId { get; set; }

        [Required]
        public string FldName { get; set; }

        [ForeignKey(nameof(Superordinate))]
        public string FldSuperordinateId { get; set; }
        public virtual TblSalesOrganization Superordinate { get; set; }
    }
}
