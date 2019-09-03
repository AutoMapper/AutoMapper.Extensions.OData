using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore
{
    [Table("TCities")]
    public class TCity
    {
        [Column("Id")]
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }

        [Column("Name")]
        public String Name { get; set; }
    }
}
