using Microsoft.EntityFrameworkCore;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    [Owned]
    public class DoorModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
    }
}
