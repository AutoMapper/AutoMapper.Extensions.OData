using Microsoft.EntityFrameworkCore;

namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    [Owned]
    public class Door
    {
        public int Id { get; set; }
        public string Name{ get; set; }
        public int AddressId { get; set; }
    }
}
