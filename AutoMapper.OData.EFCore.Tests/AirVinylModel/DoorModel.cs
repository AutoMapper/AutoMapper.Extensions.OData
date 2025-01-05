using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class DoorModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
        public int DoorManufacturerId { get; set; }
        public int DoorKnobId { get; set; }
        public DoorManufacturerModel DoorManufacturer { get; set; }
        public DoorKnobModel DoorKnob { get; set; }
    }
}
