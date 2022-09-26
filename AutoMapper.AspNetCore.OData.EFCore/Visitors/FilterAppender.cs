using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class FilterAppender : ExpressionVisitor
    {
        public FilterAppender(Expression expression, ODataExpansionOptions expansion, ODataQueryContext context)
        {
            this.expansion = expansion;
            this.expression = expression;
            this.context = context;
        }

        private readonly ODataExpansionOptions expansion;
        private readonly Expression expression;
        private readonly ODataQueryContext context;

        public static Expression AppendFilter(Expression expression, ODataExpansionOptions expansion, ODataQueryContext context)
            => new FilterAppender(expression, expansion, context).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Type elementType = expansion.MemberType.GetUnderlyingElementType();
            if (node.Method.Name == "Select"
                && elementType == node.Type.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                return Expression.Call
                (
                    node.Method.DeclaringType,
                    "Where",
                    new Type[] { node.GetUnderlyingElementType() },
                    node,
                    expansion.FilterOptions.FilterClause.GetFilterExpression(elementType, context)
                );
            }

            return base.VisitMethodCall(node);
        }
    }
}
