using LogicBuilder.Expressions.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class MethodAppender : ExpressionVisitor
    {
        public MethodAppender(Expression expression, ODataExpansionOptions expansion)
        {
            this.expansion = expansion;
            this.expression = expression;
        }

        private readonly ODataExpansionOptions expansion;
        private readonly Expression expression;

        public static Expression AppendQueryMethod(Expression expression, ODataExpansionOptions expansion)
            => new MethodAppender(expression, expansion).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Type elementType = expansion.MemberType.GetUnderlyingElementType();
            if (node.Method.Name == "Select"
                && elementType == node.Type.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                return node.GetQueryableMethod
                (
                    expansion.QueryOptions.OrderByClause,
                    elementType,
                    expansion.QueryOptions.Skip,
                    expansion.QueryOptions.Top
                );
            }

            return base.VisitMethodCall(node);
        }
    }
}
