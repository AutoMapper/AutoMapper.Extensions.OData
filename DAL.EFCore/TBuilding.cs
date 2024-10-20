using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.EFCore
{
    [Table("OB_TBuilding")]
    public class TBuilding
    {

        [Column("pkBID")]
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }

        [Column("Identifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Identity { get; set; }

        [Column("sLongName")]
        public String LongName { get; set; }

        [ForeignKey("Builder")]
        public Int32 BuilderId { get; set; }

        public TBuilder Builder { get; set; }

        public TMandator Mandator { get; set; }
        
        public string[] Parameters { get; set; }

        [ForeignKey("Mandator")]
        [Column("fkMandatorID")]
        public Int32 MandatorId { get; set; }

    }
}
