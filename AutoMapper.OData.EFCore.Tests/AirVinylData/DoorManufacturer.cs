using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class DoorManufacturer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
