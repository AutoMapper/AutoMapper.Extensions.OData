using System.ComponentModel.DataAnnotations;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int Value { get; set; }

        [Required]
        public Person RatedBy { get; set; }
       
        public int RatedByPersonId { get; set; }

        [Required]
        public int RecordStoreId { get; set; }
    }
}
