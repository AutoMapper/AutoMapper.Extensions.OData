using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    [Owned]
    public class AddressModel
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public List<int> RoomNumbers { get; set; } = [];

        public int RecordStoreId { get; set; }

        public ICollection<DoorModel> Doors { get; set; } = [];
    }
}
