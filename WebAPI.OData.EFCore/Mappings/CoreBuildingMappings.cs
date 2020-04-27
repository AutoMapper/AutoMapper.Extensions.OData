using DAL.EFCore;
using Domain.OData;

namespace WebAPI.OData.EFCore.Mappings
{
    public class CoreBuildingMappings : AutoMapper.Profile
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
        }
    }
}
