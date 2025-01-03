using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class RecordStore
    {
        [Key]
        public int RecordStoreId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public Address StoreAddress { get; set; } 

        public List<string> Tags { get; set; } = [];

        public ICollection<Rating> Ratings { get; set; } = [];         
    }
}
