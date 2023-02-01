using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EF6.Aggregation
{
    [Table("TblProduct")]
    public class TblProduct
    {
        [Key]
        public string FldId { get; set; }

        [Required, ForeignKey(nameof(Category))]
        public string FldCategoryId { get; set;}
        public TblCategory Category { get; set;}

        public virtual ICollection<TblSales> Sales { get; set; }

        [Required]
        public string FldName { get; set; }

        [Required]
        public string FldColor { get; set;}

        [Required]
        public decimal FldTaxRate { get; set; }
    }
}
