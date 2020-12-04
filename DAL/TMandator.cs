using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL
{
    [Table("G_TMandator")]
    public class TMandator
    {
        [Column("pkMandatorID")]
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }

        [Column("gIdentity")]
        public Guid Identity { get; set; }

        [Column("sName")]
        public String Name { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<TBuilding> Buildings { get; set; }

    }
}
