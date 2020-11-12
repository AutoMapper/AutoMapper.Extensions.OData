using DAL.EF6;
using Domain.OData;

namespace AutoMapper.OData.EF6.Tests.Mappings
{
    public class CoreBuildingMappings : AutoMapper.Profile
    {
        public CoreBuildingMappings()
        {
            // Dummy variables for automapper to pull the names for parameters
            string buildingParameter = null;
            int builderParameter = 0;

            CreateMap<TBuilding, CoreBuilding>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.LongName))
                .ForMember(d => d.Tenant, o => o.MapFrom(s => s.Mandator))
                .ForMember(d => d.Parameter, o => o.MapFrom(s => buildingParameter))
                .ForAllMembers(o => o.ExplicitExpansion());

            CreateMap<TBuilder, OpsBuilder>()
                .ForMember(d => d.Parameter, o => o.MapFrom(s => builderParameter))
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<TCity, OpsCity>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
}
