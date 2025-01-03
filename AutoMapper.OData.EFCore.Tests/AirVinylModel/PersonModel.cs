using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class PersonModel
    {
        [Key]
        public int PersonId { get; set; }
        
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public GenderModel Gender { get; set; }

        public int NumberOfRecordsOnWishList { get; set; }

        public decimal AmountOfCashToSpend { get; set; }

        [Contained]
        public ICollection<VinylRecordModel> VinylRecords { get; set; } = new List<VinylRecordModel>();

        public ICollection<CarModel> Cars { get; set; } = [];
    }
}
