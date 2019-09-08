﻿using DAL.EFCore;
using Domain.OData;

namespace WebAPI.OData.EFCore.Mappings
{
    public class CoreBuildingMappings : AutoMapper.Profile
    {
        public CoreBuildingMappings()
        {
            CreateMap<TBuilding, CoreBuilding>()
                .ForMember(d => d.Identity, o => o.MapFrom(s => s.Identity))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.LongName))
                .ForMember(d => d.Builder, o => o.MapFrom(s => s.Builder))
                .ForMember(d => d.Tenant, o => o.MapFrom(s => s.Mandator))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<TBuilder, OpsBuilder>();
            CreateMap<TCity, OpsCity>();
        }
    }
}
