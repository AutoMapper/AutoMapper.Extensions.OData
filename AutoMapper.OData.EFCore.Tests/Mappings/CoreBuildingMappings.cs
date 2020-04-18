using DAL.EFCore;
using Domain.OData;

namespace AutoMapper.OData.EFCore.Tests.Mappings
{
    public class CoreBuildingMappings : AutoMapper.Profile
    {
        public CoreBuildingMappings()
        {
            CreateMap<TBuilding, CoreBuilding>()
                .ForMember(d => d.Identity, o => o.MapFrom(s => s.Identity))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.LongName))
                .ForMember(d => d.Builder, o => o.ExplicitExpansion())
                .ForMember(d => d.Tenant, o => { o.MapFrom(s => s.Mandator); o.ExplicitExpansion(); })
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<TBuilder, OpsBuilder>()
                .ForMember(d => d.City, o => o.ExplicitExpansion());
            CreateMap<TCity, OpsCity>();
        }
    }
}
