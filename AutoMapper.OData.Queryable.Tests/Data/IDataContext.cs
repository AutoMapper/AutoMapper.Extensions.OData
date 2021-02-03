using System.Linq;

namespace AutoMapper.OData.Queryable.Tests.Data
{
    public interface IDataContext
    {
        IQueryable<TData> Set<TData>() where TData : class;
    }
}