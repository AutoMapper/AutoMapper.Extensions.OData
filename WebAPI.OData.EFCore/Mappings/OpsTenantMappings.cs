using DAL.EFCore;
using Domain.OData;

namespace WebAPI.OData.EFCore.Mappings
{
    public class OpsTenantMappings : AutoMapper.Profile
    {
        public OpsTenantMappings()
        {
            CreateMap<TMandator, OpsTenant>()
                .ForMember(d => d.Identity, o => o.MapFrom(s => s.Identity))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Buildings, o => { o.MapFrom(s => s.Buildings); o.ExplicitExpansion(); })
                .ForAllOtherMembers(o => o.Ignore())
                ;
        }
    }
}
