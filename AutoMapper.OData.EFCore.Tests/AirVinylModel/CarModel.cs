using Microsoft.EntityFrameworkCore;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    [Owned]
    public class CarModel
    {
        public string Name { get; set; }
        public int PersonId { get; set; }
    }
}
