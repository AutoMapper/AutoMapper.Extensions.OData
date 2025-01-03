using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class DoorManufacturerModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
