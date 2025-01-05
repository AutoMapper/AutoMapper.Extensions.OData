using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class DoorKnobModel
    {
        [Key]
        public int Id { get; set; }
        public string Style { get; set; }
    }
}
