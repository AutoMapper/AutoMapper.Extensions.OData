
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Expansions
{
    internal class ExpansionsHelper(ODataQueryContext context)
    {
        private readonly ODataQueryContext context = context;

        public ICollection<Expression<Func<TSource, object>>> BuildExplicitExpansions<TSource>(IEnumerable<List<Expansion>> includes, List<string> selects)
            where TSource : class
        {
            return GetAllExpansions([]);

            List<Expression<Func<TSource, object>>> GetAllExpansions(List<LambdaExpression> memberSelectors)
            {
                string parameterName = "i";
                ParameterExpression param = Expression.Parameter(typeof(TSource), parameterName);

                AddSelectors(memberSelectors, selects, param, param);

                return includes
                    .Select(include => BuildSelectorExpression<TSource>(include, memberSelectors, parameterName))
                    .Concat(memberSelectors.Select(selector => (Expression<Func<TSource, object>>)selector))
                    .ToList();
            }
        }

        private void AddSelectors(List<LambdaExpression> memberSelectors, List<string> selects, ParameterExpression param, Expression parentBody)
        {
            if (LogicBuilder.Expressions.Utils.TypeExtensions.IsList(parentBody.Type) || LogicBuilder.Expressions.Utils.TypeExtensions.IsLiteralType(parentBody.Type))
                return;

            memberSelectors.AddRange(GetSelectedSelectors(param, parentBody, selects));
        }

        private Expression<Func<TSource, object>> BuildSelectorExpression<TSource>(List<Expansion> fullName, List<LambdaExpression> memberSelectors, string parameterName = "i")
        {
            ParameterExpression param = Expression.Parameter(typeof(TSource), parameterName);

            return (Expression<Func<TSource, object>>)Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType([param.Type, typeof(object)]),
                BuildSelectorExpression(param, fullName, memberSelectors, parameterName),
                param
            );
        }

        //e.g. /opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))
        private Expression BuildSelectorExpression(Expression sourceExpression, List<Expansion> parts, List<LambdaExpression> memberSelectors, string parameterName = "i")
        {
            Expression parent = sourceExpression;

            //Arguments to create a nested expression when the parent expansion is a collection
            //See AddChildSeelctors() below
            List<LambdaExpression> childMemberSelectors = [];

            for (int i = 0; i < parts.Count; i++)
            {
                if (LogicBuilder.Expressions.Utils.TypeExtensions.IsList(parent.Type))
                {
                    Expression selectExpression = GetSelectExpression
                    (
                        parts.Skip(i),
                        parent,
                        childMemberSelectors,
                        parameterName
                    );

                    AddChildSeelctors();

                    return selectExpression;
                }
                else
                {
                    parent = Expression.MakeMemberAccess(parent, LogicBuilder.Expressions.Utils.TypeExtensions.GetMemberInfo(parent.Type, parts[i].MemberName));

                    if (LogicBuilder.Expressions.Utils.TypeExtensions.IsList(parent.Type))
                    {
                        ParameterExpression childParam = Expression.Parameter(LogicBuilder.Expressions.Utils.TypeExtensions.GetUnderlyingElementType(parent), GetChildParameterName(parameterName));
                        //selectors from an underlying list element must be added here.
                        AddSelectors
                        (
                            childMemberSelectors,
                            parts[i].Selects,
                            childParam,
                            childParam
                        );
                    }
                    else
                    {
                        AddSelectors(memberSelectors, parts[i].Selects, Expression.Parameter(sourceExpression.Type, parameterName), parent);
                    }
                }
            }

            AddChildSeelctors();

            return parent;

            //Adding childValueMemberSelectors created above and in a the recursive call:
            //i0 => i0.Builder.Name becomes
            //i => i.Buildings.Select(i0 => i0.Builder.Name)
            void AddChildSeelctors()
            {
                childMemberSelectors.ForEach(selector =>
                {
                    memberSelectors.Add(Expression.Lambda
                    (
                        typeof(Func<,>).MakeGenericType([sourceExpression.Type, typeof(object)]),
                        Expression.Call
                        (
                            typeof(Enumerable),
                            "Select",
                            [LogicBuilder.Expressions.Utils.TypeExtensions.GetUnderlyingElementType(parent), typeof(object)],
                            parent,
                            selector
                        ),
                        Expression.Parameter(sourceExpression.Type, parameterName)
                    ));
                });
            }
        }

        public LambdaExpression[] GetSelectedSelectors(ParameterExpression param, Expression parentBody, List<string> selects)
        {
            if (selects == null || selects.Count == 0)
            {
                DefaultExpansionsBuilder defaultExpansionsBuilder = new(context);
                return defaultExpansionsBuilder.Build(param, parentBody);
            }

            return selects
                .Select(select => LogicBuilder.Expressions.Utils.TypeExtensions.GetMemberInfo(parentBody.Type, select))
                .Select(member => Expression.MakeMemberAccess(parentBody, member))
                .Select
                (
                    selector => selector.Type.IsValueType
                        ? (Expression)Expression.Convert(selector, typeof(object))
                        : selector
                )
                .Select
                (
                    selector => Expression.Lambda
                    (
                        typeof(Func<,>).MakeGenericType([param.Type, typeof(object)]),
                        selector,
                        param
                    )
                )
                .ToArray();
        }

        private static string GetChildParameterName(string currentParameterName)
        {
            string lastChar = currentParameterName.Substring(currentParameterName.Length - 1);
            if (short.TryParse(lastChar, out short lastCharShort))
            {
                return string.Concat
                (
                    currentParameterName.Substring(0, currentParameterName.Length - 1),
                    (lastCharShort++).ToString(CultureInfo.CurrentCulture)
                );
            }
            else
            {
                return currentParameterName += "0";
            }
        }

        private Expression GetSelectExpression(IEnumerable<Expansion> expansions, Expression parent, List<LambdaExpression> valueMemberSelectors, string parameterName)
        {
            ParameterExpression parameter = Expression.Parameter(LogicBuilder.Expressions.Utils.TypeExtensions.GetUnderlyingElementType(parent), GetChildParameterName(parameterName));
            Expression selectorBody = BuildSelectorExpression(parameter, expansions.ToList(), valueMemberSelectors, parameter.Name);
            return Expression.Call
            (
                typeof(Enumerable),
                "Select",
                [parameter.Type, selectorBody.Type],
                parent,
                Expression.Lambda
                (
                    typeof(Func<,>).MakeGenericType([parameter.Type, selectorBody.Type]),
                    selectorBody,
                    parameter
                )
            );
        }
    }
}
