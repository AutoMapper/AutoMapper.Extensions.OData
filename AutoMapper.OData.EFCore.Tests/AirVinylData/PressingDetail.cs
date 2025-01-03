using System.ComponentModel.DataAnnotations;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class PressingDetail
    {
        [Key] 
        public int PressingDetailId { get; set; }
 
        [Required]
        public int Grams { get; set; }

        [Required]
        public int Inches { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }
    }
}
