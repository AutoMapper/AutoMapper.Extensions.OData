using LogicBuilder.Expressions.Utils;
using System;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class FilterAppender : ExpressionVisitor
    {
        public FilterAppender(Expression expression, ODataExpansionOptions expansion)
        {
            this.expansion = expansion;
            this.expression = expression;
        }

        private readonly ODataExpansionOptions expansion;
        private readonly Expression expression;

        public static Expression AppendFilter(Expression expression, ODataExpansionOptions expansion)
            => new FilterAppender(expression, expansion).Visit(expression);

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
                    expansion.FilterOptions.FilterClause.GetFilterExpression(elementType)
                );
            }

            return base.VisitMethodCall(node);
        }
    }
}
