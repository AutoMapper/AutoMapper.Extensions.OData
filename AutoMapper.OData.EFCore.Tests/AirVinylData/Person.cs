using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//Adapted from https://github.com/KevinDockx/BuildingAnODataAPIAspNetCore
namespace AutoMapper.OData.EFCore.Tests.AirVinylData
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        
        [StringLength(100)]    
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public int NumberOfRecordsOnWishList { get; set; }

        public decimal AmountOfCashToSpend { get; set; }

        [Contained]
        public ICollection<VinylRecord> VinylRecords { get; set; } = new List<VinylRecord>();

        public ICollection<Car> Cars { get; set; } = [];
    }
}
