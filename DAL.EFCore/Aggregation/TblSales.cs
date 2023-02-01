using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore.Aggregation
{
    [Table("TblSales")]
    public class TblSales
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FldId { get; set; }

        [Required, ForeignKey(nameof(Customer))]
        public string FldCustomerId { get; set; }
        public virtual TblCustomer Customer { get; set; }

        [Required, ForeignKey(nameof(Time))]
        public string FldTimeId { get; set; }
        public virtual TblTime Time { get; set; }

        [Required, ForeignKey(nameof(Product))]
        public string FldProductId { get; set; }
        public virtual TblProduct Product { get; set; }

        [Required, ForeignKey(nameof(SalesOrganization))]
        public string FldSalesOrganizationId { get; set; }
        public virtual TblSalesOrganization SalesOrganization { get; set; }

        [Required, ForeignKey(nameof(Currency))]
        public string FldCurrencyCode { get; set; }
        public virtual TblCurrency Currency { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal FldAmount { get; set; }
    }
}
