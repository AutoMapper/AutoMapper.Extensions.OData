using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class DoorKnob
    {
        [Key]
        public int Id { get; set; }
        public string Style { get; set; }
    }
}
