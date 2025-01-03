using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class PressingDetailModel
    {
        [Key] 
        public int PressingDetailId { get; set; }
 
        public int Grams { get; set; }
        public int Inches { get; set; }

        public string Description { get; set; }
    }
}
