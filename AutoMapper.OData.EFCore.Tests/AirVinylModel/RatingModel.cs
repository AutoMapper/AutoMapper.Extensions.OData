using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class RatingModel
    {
        [Key]
        public int RatingId { get; set; }

        public int Value { get; set; }

        public PersonModel RatedBy { get; set; }
       
        public int RatedByPersonId { get; set; }

        public int RecordStoreId { get; set; }
    }
}
