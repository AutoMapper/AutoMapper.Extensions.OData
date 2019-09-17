using System.Collections.Generic;
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
                .ForMember(d => d.Parameter, o => o.MapFrom((s, d, m, c) => (string)c.Items.GetOrDefault("parameter", "unknown")))
                //.ForMember(d => d.Parameter, o => o.MapFrom(new VR(), s => s.LongName))
                //.ForMember(d => d.Parameter, o => o.MapFrom(s => "a"))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<TBuilder, OpsBuilder>();
            CreateMap<TCity, OpsCity>();
        }
    }

    public static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value = default(TValue))
        {
            return dictionary.TryGetValue(key, out TValue v) ? v : value;
        }
    }
}
