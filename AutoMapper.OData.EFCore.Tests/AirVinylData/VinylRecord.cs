using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class VinylRecord
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

        public PressingDetail PressingDetail { get; set; }

        public int PressingDetailId { get; set; }

        public virtual Person Person { get; set; }

        public int PersonId { get; set; }

        public ICollection<DynamicProperty> DynamicVinylRecordProperties { get; set; } 
            = new List<DynamicProperty>();

        private IDictionary<string, object> _properties;
        [NotMapped]
        public IDictionary<string, object> Properties
        {
            get
            {
                // create dictionary from DynamicVinylProperties           
                if (_properties == null)
                {
                    _properties = new Dictionary<string, object>();
                    foreach (var dynamicProperty in DynamicVinylRecordProperties)
                    {
                        _properties.Add(dynamicProperty.Key, dynamicProperty.Value);
                    }
                }

                return _properties;
            }
            set
            {
                _properties = value;
            }
        }
    }
}
