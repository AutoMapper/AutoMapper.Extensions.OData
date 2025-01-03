using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class RecordStoreModel
    {
        [Key]
        public int RecordStoreId { get; set; }

        public string Name { get; set; }

        public AddressModel StoreAddress { get; set; } 

        public List<string> Tags { get; set; } = [];

        public ICollection<RatingModel> Ratings { get; set; } = [];         
    }
}
