using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class ChildCollectionOrderByUpdater : ProjectionVisitor
    {
        private readonly ODataQueryContext context;

        public ChildCollectionOrderByUpdater(List<ODataExpansionOptions> expansions, ODataQueryContext context) 
            : base(expansions)
        {
            this.context = context;
        }

        public static Expression UpdaterExpansion(Expression expression, List<ODataExpansionOptions> expansions, ODataQueryContext context)
                => new ChildCollectionOrderByUpdater(expansions, context).Visit(expression);

        protected override Expression GetBindingExpression(MemberAssignment binding, ODataExpansionOptions expansion)
        {
            if (expansion.QueryOptions != null)
            {
                return MethodAppender.AppendQueryMethod(binding.Expression, expansion, context);
            }
            else if (expansions.Count > 1)  //Mutually exclusive with expansion.QueryOptions != null.                            
            {                               //There can be only one set of QueryOptions in the list.  See the GetQueryMethods() method in QueryableExtensions.UpdateQueryable.
                return UpdaterExpansion
                (                             
                    binding.Expression,
                    expansions.Skip(1).ToList(),
                    context
                );
            }
            else
                throw new ArgumentException("Last expansion in the list must have a filter", nameof(expansions));
        }
    }
}
