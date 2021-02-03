using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class ChildCollectionOrderByUpdater : ProjectionVisitor
    {
        public ChildCollectionOrderByUpdater(List<ODataExpansionOptions> expansions) : base(expansions)
        {
        }

        public static Expression UpdaterExpansion(Expression expression, List<ODataExpansionOptions> expansions)
                => new ChildCollectionOrderByUpdater(expansions).Visit(expression);

        protected override Expression GetBindingExpression(MemberAssignment binding, ODataExpansionOptions expansion)
        {
            if (expansion.QueryOptions != null)
            {
                return MethodAppender.AppendQueryMethod(binding.Expression, expansion);
            }
            else if (expansions.Count > 1)  //Mutually exclusive with expansion.QueryOptions != null.                            
            {                               //There can be only one set of QueryOptions in the list.  See the GetQueryMethods() method in QueryableExtensions.UpdateQueryable.
                return UpdaterExpansion
                (
                    binding.Expression,
                    expansions.Skip(1).ToList()
                );
            }
            else
                throw new ArgumentException("Last expansion in the list must have a filter", nameof(expansions));
        }
    }
}
