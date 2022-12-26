using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore.Aggregation
{
    [Table("TblSales")]
    public class TblSales
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FldId { get; set; }

        [Required]
        public string FldCustomerId { get; set; }

        [ForeignKey(nameof(FldCustomerId))]
        public virtual TblCustomer Customer { get; set; }

        [Required]
        public string FldTimeId { get; set; }

        [ForeignKey(nameof(FldTimeId))]
        public virtual TblTime Time { get; set; }

        [Required]
        public string FldProductId { get; set; }

        [ForeignKey(nameof(FldProductId))]
        public virtual TblProduct Product { get; set; }

        [Required]
        public string FldSalesOrganizationId { get; set; }

        [ForeignKey(nameof(FldSalesOrganizationId))]
        public virtual TblSalesOrganization SalesOrganization { get; set; }

        [Required]
        public string FldCurrencyCode { get; set; }

        [ForeignKey(nameof(FldCurrencyCode))]
        public virtual TblCurrency Currency { get; set; }

        [Required]
        public decimal FldAmount { get; set; }
    }
}
