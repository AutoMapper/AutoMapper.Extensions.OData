using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.Internal;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class FilterAppender : ExpressionVisitor
    {
        public FilterAppender(Expression expression, ODataExpansionOptions expansion, ODataQueryContext context, IMapper mapper)
        {
            this.expansion = expansion;
            this.expression = expression;
            this.context = context;
            this.mapper = mapper;
        }

        private readonly ODataExpansionOptions expansion;
        private readonly Expression expression;
        private readonly ODataQueryContext context;
        private readonly IMapper mapper;

        public static Expression AppendFilter(Expression expression, ODataExpansionOptions expansion, ODataQueryContext context, IMapper mapper)
            => new FilterAppender(expression, expansion, context, mapper).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Type elementType = expansion.MemberType.GetUnderlyingElementType();
            if (node.Method.Name == "Select"
                && elementType == node.Type.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                Type parentUnderlyingType = node.Arguments[0].Type.GetUnderlyingElementType();
                Type nodeUnderlyingType = elementType;
                LambdaExpression filter = GetFilterExpression();
                var replacedParent = GetNewParentExpression();
                var listOfArgumentsForNewMethod = GetArgumentsForNewMethod();

                return Expression.Call
                (
                    node.Method.DeclaringType,
                    node.Method.Name,
                    node.Method.GetGenericArguments(),
                    listOfArgumentsForNewMethod
                );

                LambdaExpression GetFilterExpression()
                {
                    LambdaExpression filterExpression = expansion.FilterOptions.FilterClause.GetFilterExpression(elementType, context);

                    if (parentUnderlyingType != nodeUnderlyingType)
                    {
                        var typeMap = mapper.ConfigurationProvider.Internal().ResolveTypeMap(sourceType: parentUnderlyingType, destinationType: nodeUnderlyingType);
                        if (typeMap != null)
                        {
                            Type sourceType = typeof(Func<,>).MakeGenericType(nodeUnderlyingType, typeof(bool));
                            Type destType = typeof(Func<,>).MakeGenericType(parentUnderlyingType, typeof(bool));
                            Type sourceExpressionype = typeof(Expression<>).MakeGenericType(sourceType);
                            Type destExpressionType = typeof(Expression<>).MakeGenericType(destType);
                            filterExpression = mapper.MapExpression(filterExpression, sourceExpressionype, destExpressionType);
                        }
                    }

                    return filterExpression;
                }

                Expression GetNewParentExpression()
                {
                    return new ReplaceExpressionVisitor
                    (
                        node.Arguments[0],
                        Expression.Call
                        (
                            node.Method.DeclaringType,
                            "Where",
                            [parentUnderlyingType],
                            node.Arguments[0],
                            filter
                        )
                    ).Visit(node.Arguments[0]);
                }

                Expression[] GetArgumentsForNewMethod()
                {
                    return 
                    [
                        .. node.Arguments.Aggregate(new List<Expression>(), (lst, next) =>
                        {
                            if (next == node.Arguments[0])
                                lst.Add(replacedParent);
                            else
                                lst.Add(next);
                            return lst;
                        })
                    ];
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
