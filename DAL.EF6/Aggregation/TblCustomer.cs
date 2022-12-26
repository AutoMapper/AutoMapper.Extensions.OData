using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EF6.Aggregation
{
    [Table("TblCustomer")]
    public class TblCustomer
    {
        [Key]
        public string FldId { get; set; }

        [Required]
        public string FldName { get; set; }

        public string FldCountry { get; set; }

        public virtual ICollection<TblSales> Sales { get; set; }
    }
}
