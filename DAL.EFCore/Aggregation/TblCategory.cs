using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore.Aggregation
{
    [Table("TblCategory")]
    public class TblCategory
    {
        [Key]
        public string FldId { get; set; }

        [Required]
        public string FldName { get; set; }

        public virtual ICollection<TblProduct> Products { get; set; }
    }
}
