using Microsoft.AspNetCore.OData.Query;
using System.Linq;

namespace WebAPI.OData.EFCore.Attributes
{
    public class AutomapperEnableQueryAttribute : EnableQueryAttribute
    {
        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            var ignoreQueryOptions =
                AllowedQueryOptions.Skip |
                AllowedQueryOptions.Top |
                AllowedQueryOptions.Filter |
                AllowedQueryOptions.Expand |
                AllowedQueryOptions.Select |
                AllowedQueryOptions.OrderBy |
                AllowedQueryOptions.Count;

            return queryOptions.ApplyTo(queryable, ignoreQueryOptions);
        }
    }
}
