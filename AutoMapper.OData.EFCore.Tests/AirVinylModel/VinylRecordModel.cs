using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class VinylRecordModel
    {
        [Key]
        public int VinylRecordId { get; set; }

        [StringLength(150)]
        [Required]
        public string Title { get; set; }

        [StringLength(150)]
        [Required]
        public string Artist { get; set; }

        [StringLength(50)]
        public string CatalogNumber { get; set; }

        public int? Year { get; set; }

        public PressingDetailModel PressingDetail { get; set; }

        public int PressingDetailId { get; set; }

        public virtual PersonModel Person { get; set; }

        public int PersonId { get; set; }

        public ICollection<DynamicPropertyModel> DynamicVinylRecordProperties { get; set; } 
            = new List<DynamicPropertyModel>();

        public IDictionary<string, object> Properties { get; set; }
    }
}
