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
                .ForMember(d => d.Builder, o => o.MapFrom(s => s.Builder))
                .ForMember(d => d.Tenant, o => o.MapFrom(s => s.Mandator))
                .ForMember(d => d.Parameter, o => o.MapFrom((s, d, m, c) => c.Items.ContainsKey("parameter") ? (string)c.Items["parameter"] : "unknown"))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<TBuilder, OpsBuilder>();
            CreateMap<TCity, OpsCity>();
        }
    }
}
