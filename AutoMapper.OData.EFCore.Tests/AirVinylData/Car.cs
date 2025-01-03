using Microsoft.EntityFrameworkCore;

namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    [Owned]
    public class Car
    {
        public string Name { get; set; }
        public int PersonId { get; set; }
    }
}
