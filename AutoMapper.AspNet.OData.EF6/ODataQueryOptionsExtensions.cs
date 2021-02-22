using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using System;

namespace AutoMapper.AspNet.OData
{
    internal static class ODataQueryOptionsExtensions
    {
        /// <summary>
        /// Adds the expand options to the result.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="options"></param>
        public static void AddExpandOptionsResult<TModel>(this ODataQueryOptions<TModel> options)
        {
            if (options.SelectExpand == null)
                return;
            options.Request.ODataProperties().SelectExpandClause = options.SelectExpand.SelectExpandClause;
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

            options.Request.ODataProperties().TotalCount = longCount;
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

            options.Request.ODataProperties().NextLink = options.Request.GetNextPageLink(pageSize);
        }
    }
}
