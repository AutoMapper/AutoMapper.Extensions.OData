using AutoMapper;
using DAL.EF6;
using Domain.OData;

namespace WebAPI.OData.EF6.Mappings
{
    public class OpsTenantMappings : Profile
    {
        public OpsTenantMappings()
        {
            CreateMap<TMandator, OpsTenant>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
}
