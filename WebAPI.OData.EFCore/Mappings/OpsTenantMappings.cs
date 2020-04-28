using DAL.EFCore;
using Domain.OData;

namespace WebAPI.OData.EFCore.Mappings
{
    public class OpsTenantMappings : AutoMapper.Profile
    {
        public OpsTenantMappings()
        {
            CreateMap<TMandator, OpsTenant>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
}
