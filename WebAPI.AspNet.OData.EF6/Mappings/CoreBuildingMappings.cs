using AutoMapper;
using DAL;
using Domain.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.AspNet.OData.EF6.Mappings
{
    public class CoreBuildingMappings : Profile
    {
        public CoreBuildingMappings()
        {
            CreateMap<TBuilding, CoreBuilding>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.LongName))
                .ForMember(d => d.Tenant, o => o.MapFrom(s => s.Mandator))
                .ForAllMembers(o => o.ExplicitExpansion());

            CreateMap<TBuilder, OpsBuilder>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<TCity, OpsCity>()
                .ForAllMembers(o => o.ExplicitExpansion());

            CreateMap<OpsCity, TCity>();
        }
    }
}