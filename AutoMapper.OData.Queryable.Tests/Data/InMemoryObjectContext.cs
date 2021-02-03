using System.Collections.Generic;
using System.Linq;
using DAL.EFCore;

namespace AutoMapper.OData.Queryable.Tests.Data
{
    public class InMemoryObjectContext : IDataContext
    {
        private IDictionary<string, object> storage = new Dictionary<string, object>();

        public IList<TMandator> MandatorSet => GetBacking<TMandator>();

        public IList<TBuilding> BuildingSet => GetBacking<TBuilding>();

        public IList<TBuilder> Builder => GetBacking<TBuilder>();

        public IList<TCity> City => GetBacking<TCity>();

        private IList<TData> GetBacking<TData>() where TData : class
        {
            var name = typeof(TData).ToString();
            if (!storage.ContainsKey(name))
                storage[name] = new List<TData>();

            return (List<TData>)storage[name];
        }

        public IQueryable<TData> Set<TData>() where TData : class => 
            GetBacking<TData>().AsQueryable();
    }
}