using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class ChildCollectionFilterUpdater : ProjectionVisitor
    {
        public ChildCollectionFilterUpdater(List<ODataExpansionOptions> expansions, ODataQueryContext context) : base(expansions)
        {
            this.context = context;
        }

        private readonly ODataQueryContext context;

        public static Expression UpdaterExpansion(Expression expression, List<ODataExpansionOptions> expansions, ODataQueryContext context)
                => new ChildCollectionFilterUpdater(expansions, context).Visit(expression);

        protected override Expression GetBindingExpression(MemberAssignment binding, ODataExpansionOptions expansion)
        {
            if (expansion.FilterOptions != null)
            {
                return FilterAppender.AppendFilter(binding.Expression, expansion, context);
            }
            else if (expansions.Count > 1)  //Mutually exclusive with expansion.Filter != null.                            
            {                               //There can be only one filter in the list.  See the GetFilters() method in QueryableExtensions.UpdateQueryable.
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
