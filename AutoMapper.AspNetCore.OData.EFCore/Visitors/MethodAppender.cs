using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class MethodAppender : ExpressionVisitor
    {
        private readonly ODataQueryContext context;

        public MethodAppender(Expression expression, ODataExpansionOptions expansion, ODataQueryContext context)
        {
            this.expansion = expansion;
            this.expression = expression;
            this.context = context;
        }

        private readonly ODataExpansionOptions expansion;
        private readonly Expression expression;

        public static Expression AppendQueryMethod(Expression expression, ODataExpansionOptions expansion, ODataQueryContext context)
            => new MethodAppender(expression, expansion, context).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Type elementType = expansion.MemberType.GetUnderlyingElementType();
            if (node.Method.Name == "Select"
                && elementType == node.Type.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                return node.GetQueryableMethod
                (
                    context,
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
