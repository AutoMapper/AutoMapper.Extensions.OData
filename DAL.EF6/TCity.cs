using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EF6
{
    public class TCity
    {
        [Column("Id")]
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }

        [Column("Name")]
        public String Name { get; set; }
    }
}
