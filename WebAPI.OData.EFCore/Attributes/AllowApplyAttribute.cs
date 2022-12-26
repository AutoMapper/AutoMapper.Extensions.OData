using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Linq;

namespace WebAPI.OData.EFCore.Attributes
{
    public class AllowApplyAttribute : EnableQueryAttribute
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

        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext is null)
            {
                throw new ArgumentNullException(nameof(actionExecutedContext));
            }

            if (actionExecutedContext.HttpContext.Response != null &&
                IsSuccessStatusCode(actionExecutedContext.HttpContext.Response.StatusCode) &&
                actionExecutedContext.Result is ObjectResult content &&
                content.Value != null &&
                content.DeclaredType == null)
            {
                // To help the `ODataOutputFormatter` to determine the correct output class
                // the `content.DeclaredType` needs to be set before appling the `$apply`-option
                // so that a valid `OData`-result is produced
                // https://github.com/OData/AspNetCoreOData/blob/4de92f52a346606a447ec4df96c5f3cd05642f50/src/Microsoft.AspNetCore.OData/Formatter/ODataOutputFormatter.cs#L127-L131
                content.DeclaredType = content.Value.GetType();
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }
    }
}
