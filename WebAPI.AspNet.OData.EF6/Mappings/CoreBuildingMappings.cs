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