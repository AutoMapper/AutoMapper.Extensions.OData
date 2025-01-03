﻿using AutoMapper.OData.EFCore.Tests.AirVinylData;
using AutoMapper.OData.EFCore.Tests.AirVinylModel;

namespace AutoMapper.OData.EFCore.Tests.Mappings
{
    public class AirVinylMappings : AutoMapper.Profile
    {
        public AirVinylMappings()
        {
            CreateMap<Address, AddressModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Car, CarModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Door, DoorModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DoorManufacturer, DoorManufacturerModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DynamicProperty, DynamicPropertyModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Person, PersonModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<PressingDetail, PressingDetailModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Rating, RatingModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<RecordStore, RecordStoreModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<SpecializedRecordStore, SpecializedRecordStoreModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<VinylRecord, VinylRecordModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
}