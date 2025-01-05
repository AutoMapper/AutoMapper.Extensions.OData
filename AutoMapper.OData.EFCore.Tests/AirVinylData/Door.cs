using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class Door
    {
        [Key]
        public int Id { get; set; }
        public string Name{ get; set; }
        public int AddressId { get; set; }
        public int DoorManufacturerId { get; set; }
        public int DoorKnobId { get; set; }
        public DoorManufacturer DoorManufacturer { get; set; }
        public DoorKnob DoorKnob { get; set; }
    }
}
