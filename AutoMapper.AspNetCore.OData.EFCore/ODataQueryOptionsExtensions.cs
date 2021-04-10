using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;

namespace AutoMapper.AspNet.OData
{
    internal static class ODataQueryOptionsExtensions
    {
        /// <summary>
        /// Adds the expand options to the result.
        /// </summary>
        /// <param name="options"></param>
        public static void AddExpandOptionsResult(this ODataQueryOptions options)
        {
            if (options.SelectExpand == null)
                return;

            options.Request.ODataFeature().SelectExpandClause = options.SelectExpand.SelectExpandClause;
        }

        /// <summary>
        /// Adds the count options to the result.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="longCount"></param>
        public static void AddCountOptionsResult(this ODataQueryOptions options, long longCount)
        {
            if (options.Count?.Value != true)
                return;

            options.Request.ODataFeature().TotalCount = longCount;
        }

        /// <summary>
        /// Adds the next link options to the result.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="pageSize"></param>
        public static void AddNextLinkOptionsResult(this ODataQueryOptions options, int pageSize)
        {
            if (options.Request == null)
                return;

            options.Request.ODataFeature().NextLink = options.Request.GetNextPageLink(pageSize, null, null);
        }
    }
}
