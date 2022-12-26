using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EF6.Aggregation
{
    [Table("TblSalesOrganization")]
    public class TblSalesOrganization
    {
        [Key]
        public string FldId { get; set; }

        [Required]
        public string FldName { get; set; }

        public string FldSuperordinateId { get; set; }

        [ForeignKey(nameof(FldSuperordinateId))]
        public virtual TblSalesOrganization Superordinate { get; set; }
    }
}
