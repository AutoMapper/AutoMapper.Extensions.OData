using System;
using System.Linq.Expressions;
using Domain.OData;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;

namespace WebAPI.OData.EFCore.Binders
{
    public class OpsTenantSearchBinder : QueryBinder, ISearchBinder
    {
        public Expression BindSearch(SearchClause searchClause, QueryBinderContext context)
        {
            SearchTermNode node = searchClause.Expression as SearchTermNode;
            Expression<Func<OpsTenant, bool>> exp = p => p.Name == node.Text;
            return exp;
        }
    }
}