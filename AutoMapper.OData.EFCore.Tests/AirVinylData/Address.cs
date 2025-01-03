using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    // Address is an owned type (no key) - used to be called complex type in EF.
    [Owned]
    public class Address
    {

        [StringLength(200)]
        public string Street { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(10)]
        public string PostalCode { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        public List<int> RoomNumbers { get; set; } = [];

        public int RecordStoreId { get; set; }

        public virtual ICollection<Door> Doors { get; set; } = [];
    }
}
