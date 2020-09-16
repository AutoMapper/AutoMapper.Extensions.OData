﻿using AutoMapper.AspNet.OData.Visitors;
using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Returns a lambda expresion representing the filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterOption"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> ToFilterExpression<T>(this FilterQueryOption filterOption, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
        {
            if (filterOption == null)
                return null;

            IQueryable queryable = Enumerable.Empty<T>().AsQueryable();
            queryable = filterOption.ApplyTo(queryable, new ODataQuerySettings() { HandleNullPropagation = handleNullPropagation });
            MethodCallExpression whereMethodCallExpression = (MethodCallExpression)queryable.Expression;

            return (Expression<Func<T, bool>>)(whereMethodCallExpression.Arguments[1].Unquote() as LambdaExpression);
        }

        public static Expression<Func<IQueryable<T>, long>> GetCountExpression<T>(Expression filter = null)
        {
            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            return Expression.Lambda<Func<IQueryable<T>, long>>(GetLongCountMethod(param, filter), param);
        }

        private static Expression GetLongCountMethod(ParameterExpression param, Expression filter = null)
        {
            return Expression.Call
            (
                typeof(Queryable),
                "LongCount",
                new Type[] { param.GetUnderlyingElementType() },
                filter == null 
                    ? new Expression[] { param } 
                    : new Expression[] { param, filter }
            );
        }

        /// <summary>
        /// Returns a lambda expresion for order and paging expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> GetQueryableExpression<T>(this ODataQueryOptions<T> options)
        {
            if (options.OrderBy == null && options.Top == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>
            (
                param.GetOrderByMethod(options), param
            );
        }

        [Obsolete("Use \"public static Expression GetOrderByMethod<T>(this Expression expression, ODataQueryOptions<T> options)\" instead.")]
        public static Expression GetOrderByMethod<T>(this ODataQueryOptions<T> options, Expression expression)
            => expression.GetOrderByMethod<T>(options);

        public static Expression GetOrderByMethod<T>(this Expression expression, ODataQueryOptions<T> options)
        {
            if (options.OrderBy == null && options.Top == null)
                return null;

            return expression.GetQueryableMethod
            (
                options.OrderBy?.OrderByClause,
                typeof(T),
                options.Skip?.Value,
                options.Top?.Value
            );
        }

        public static Expression GetQueryableMethod(this Expression expression, OrderByClause orderByClause, Type type, int? skip, int? top)
        {
            if (orderByClause == null && !top.HasValue)
                return null;

            if (orderByClause == null)
            {
                return Expression.Call
                (
                    typeof(Queryable),
                    "Take",
                    new[] { type },
                    expression,
                    Expression.Constant(top.Value)
                );
            }

            return expression
                .GetOrderByCall(orderByClause)
                .GetSkipCall(skip)
                .GetTakeCall(top);
        }

        private static Expression GetOrderByCall(this Expression expression, OrderByClause orderByClause)
        {
            const string OrderBy = "OrderBy";
            const string OrderByDescending = "OrderByDescending";

            return orderByClause.ThenBy == null
                ? GetMethodCall()
                : GetMethodCall().GetThenByCall(orderByClause.ThenBy);

            Expression GetMethodCall()
            {
                SingleValueNode orderByNode = orderByClause.Expression;
                switch (orderByNode)
                {
                    case CountNode countNode:
                        return expression.GetOrderByCountCall
                        (
                            countNode.GetPropertyPath(),
                            orderByClause.Direction == OrderByDirection.Ascending
                                ? OrderBy
                                : OrderByDescending,
                            orderByClause.RangeVariable.Name
                        );
                    default:
                        SingleValuePropertyAccessNode propertyNode = (SingleValuePropertyAccessNode)orderByNode;
                        return expression.GetOrderByCall
                        (
                            propertyNode.GetPropertyPath(),
                            orderByClause.Direction == OrderByDirection.Ascending
                                ? OrderBy
                                : OrderByDescending,
                            orderByClause.RangeVariable.Name
                        );
                }
            }
        }

        private static Expression GetThenByCall(this Expression expression, OrderByClause orderByClause)
        {
            const string ThenBy = "ThenBy";
            const string ThenByDescending = "ThenByDescending";

            return orderByClause.ThenBy == null
                ? GetMethodCall()
                : GetMethodCall().GetThenByCall(orderByClause.ThenBy);

            Expression GetMethodCall()
            {
                return orderByClause.Expression switch
                {
                    CountNode countNode => expression.GetOrderByCountCall
                    (
                        countNode.GetPropertyPath(),
                        orderByClause.Direction == OrderByDirection.Ascending
                            ? ThenBy
                            : ThenByDescending
                    ),
                    SingleValuePropertyAccessNode propertyNode => expression.GetOrderByCall
                    (
                        propertyNode.GetPropertyPath(),
                        orderByClause.Direction == OrderByDirection.Ascending
                            ? ThenBy
                            : ThenByDescending
                    ),
                    _ => throw new ArgumentException($"Unsupported SingleValueNode value: {orderByClause.Expression.GetType()}"),
                };
            }
        }

        private static string GetPropertyPath(this CountNode countNode)
        {
            switch (countNode.Source)
            {
                case CollectionNavigationNode navigationNode:
                    return string.Join(".", new List<string>().GetReferencePath(navigationNode.Source, navigationNode.NavigationProperty.Name));
                case null:
                    throw new ArgumentNullException(nameof(countNode.Source));
                default:
                    throw new ArgumentOutOfRangeException(nameof(countNode.Source));
            }
        }

        public static string GetPropertyPath(this SingleValuePropertyAccessNode singleValuePropertyAccess)
            => singleValuePropertyAccess.Source switch
            {
                SingleNavigationNode navigationNode => $"{navigationNode.GetPropertyPath()}.{singleValuePropertyAccess.Property.Name}",
                SingleComplexNode complexNode => $"{complexNode.GetPropertyPath()}.{singleValuePropertyAccess.Property.Name}",
                _ => singleValuePropertyAccess.Property.Name,
            };

        public static string GetPropertyPath(this CollectionPropertyAccessNode collectionPropertyAccess)
            => collectionPropertyAccess.Source switch
            {
                SingleNavigationNode navigationNode => $"{navigationNode.GetPropertyPath()}.{collectionPropertyAccess.Property.Name}",
                SingleComplexNode complexNode => $"{complexNode.GetPropertyPath()}.{collectionPropertyAccess.Property.Name}",
                _ => collectionPropertyAccess.Property.Name,
            };

        public static string GetPropertyPath(this SingleNavigationNode singleNavigationNode)
            => $"{string.Join(".", new List<string>().GetReferencePath(singleNavigationNode.Source, singleNavigationNode.NavigationProperty.Name))}";

        public static string GetPropertyPath(this SingleComplexNode singleComplexNode)
            => $"{string.Join(".", new List<string>().GetReferencePath(singleComplexNode.Source, singleComplexNode.Property.Name))}";

        public static string GetPropertyPath(this CollectionComplexNode collectionComplexNode)
            => $"{string.Join(".", new List<string>().GetReferencePath(collectionComplexNode.Source, collectionComplexNode.Property.Name))}";

        public static string GetPropertyPath(this CollectionNavigationNode collectionNavigationNode)
            => $"{string.Join(".", new List<string>().GetReferencePath(collectionNavigationNode.Source, collectionNavigationNode.NavigationProperty.Name))}";

        public static List<string> GetReferencePath(this List<string> list, SingleResourceNode singleResourceNode, string propertyName)
        {
            switch (singleResourceNode)
            {
                case SingleNavigationNode sourceNode:
                    list.GetReferencePath(sourceNode.Source, sourceNode.NavigationProperty.Name);
                    list.Add(propertyName);
                    return list;
                case SingleComplexNode complexNode:
                    list.GetReferencePath(complexNode.Source, complexNode.Property.Name);
                    list.Add(propertyName);
                    return list;
                default:
                    list.Add(propertyName);
                    return list;
            }
        }

        public static Expression GetSkipCall(this Expression expression, SkipQueryOption skip)
        {
            if (skip == null) return expression;

            return expression.GetSkipCall(skip.Value);
        }

        public static Expression GetTakeCall(this Expression expression, TopQueryOption top)
        {
            if (top == null) return expression;

            return expression.GetTakeCall(top.Value);
        }

        public static Expression GetSkipCall(this Expression expression, int? skip)
        {
            if (skip == null) return expression;

            return Expression.Call
            (
                expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
                "Skip",
                new[] { expression.GetUnderlyingElementType() },
                expression,
                Expression.Constant(skip.Value)
            );
        }

        public static Expression GetTakeCall(this Expression expression, int? top)
        {
            if (top == null) return expression;

            return Expression.Call
            (
                expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
                "Take",
                new[] { expression.GetUnderlyingElementType() },
                expression,
                Expression.Constant(top.Value)
            );
        }

        public static LambdaExpression MakeLambdaExpression(this ParameterExpression param, Expression body)
        {
            Type[] typeArgs = new[] { param.Type, body.Type };//Generic arguments e.g. T1 and T2 MethodName<T1, T2>(method arguments)
            Type delegateType = typeof(Func<,>).MakeGenericType(typeArgs);//Delegate type for the selector expression.  It takes a TSource and returns the sort property type
            return Expression.Lambda(delegateType, body, param);//Resulting lambda expression for the selector.
        }

        public static Expression GetOrderByCountCall(this Expression expression, string memberFullName, string methodName, string selectorParameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            ParameterExpression param = Expression.Parameter(sourceType, selectorParameterName);
            Expression countSelector = param.MakeSelector(memberFullName).GetCountCall();
            return Expression.Call
            (
                expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
                methodName,
                new Type[] { sourceType, countSelector.Type },
                expression,
                param.MakeLambdaExpression
                (
                    countSelector
                )
            );
        }

        public static Expression GetOrderByCall(this Expression expression, string memberFullName, string methodName, string selectorParameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            MemberInfo memberInfo = sourceType.GetMemberInfoFromFullName(memberFullName);
            return Expression.Call
            (
                expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
                methodName,
                new Type[] { sourceType, memberInfo.GetMemberType() },
                expression,
                memberFullName.GetTypedSelector(sourceType, selectorParameterName)
            );
        }

        /// <summary>
        /// Get Selects
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static List<string> GetSelects(this SelectExpandQueryOption clause)
        {
            if (clause == null)
                return new List<string>();

            return clause.SelectExpandClause.GetSelects();
        }

        private static List<string> GetSelects(this SelectExpandClause clause)
        {
            if (clause == null)
                return new List<string>();

            return clause.SelectedItems
                        .OfType<PathSelectItem>()
                        .Select(item => item.SelectedPath.FirstSegment.Identifier)//Only first segment is necessary because of the new syntax $expand=Builder($expand=City) vs $expand=Builder/City
                        .ToList();
        }

        /// <summary>
        /// Creates a period delimited list of navigation properties
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static List<string> GetIncludes(this SelectExpandQueryOption clause)
        {
            if (clause?.SelectExpandClause == null)
                return new List<string>();

            return clause.SelectExpandClause.SelectedItems.GetIncludes();
        }

        private static List<string> GetNestedIncludes(this ExpandedNavigationSelectItem node)
        {
            if (node == null)
                return new List<string>();

            return node.SelectAndExpand.SelectedItems.GetIncludes();
        }

        private static List<string> GetIncludes(this IEnumerable<SelectItem> selectedItems)
        {
            if (selectedItems == null)
                return new List<string>();

            return selectedItems.OfType<ExpandedNavigationSelectItem>().Aggregate(new List<string>(), (list, next) =>
            {
                string path = next.PathToNavigationProperty.FirstSegment.Identifier;//Only first segment is necessary because of the new syntax $expand=Builder($expand=City) vs $expand=Builder/City

                IEnumerable<string> navigationItems = next.GetNestedIncludes().Select(i => string.Concat(path, ".", i));
                if (navigationItems.Any())
                    list.AddRange(navigationItems);
                else
                    list.Add(path);
                return list;
            });
        }

        public static List<List<ODataExpansionOptions>> GetExpansions(this SelectExpandQueryOption clause, Type parentType)
        {
            if (clause?.SelectExpandClause == null)
                return new List<List<ODataExpansionOptions>>();

            return clause.SelectExpandClause.SelectedItems.GetExpansions(new HashSet<string>(clause.GetSelects()), parentType);
        }

        private static List<List<ODataExpansionOptions>> GetNestedExpansions(this ExpandedNavigationSelectItem node, Type type)
        {
            if (node == null)
                return new List<List<ODataExpansionOptions>>();

            return node.SelectAndExpand.SelectedItems.GetExpansions(new HashSet<string>(node.SelectAndExpand.GetSelects()), type);
        }

        private static bool ExpansionIsValid(this HashSet<string> siblingSelects, string expansion)
        {
            if (!siblingSelects.Any())
                return true;

            return siblingSelects.Contains(expansion);
        }

        private static List<List<ODataExpansionOptions>> GetExpansions(this IEnumerable<SelectItem> selectedItems, HashSet<string> selects, Type parentType)
        {
            if (selectedItems == null)
                return new List<List<ODataExpansionOptions>>();

            return selectedItems.OfType<ExpandedNavigationSelectItem>().Aggregate(new List<List<ODataExpansionOptions>>(), (listOfExpansionLists, next) =>
            {
                string path = next.PathToNavigationProperty.FirstSegment.Identifier;//Only first segment is necessary because of the new syntax $expand=Builder($expand=City) vs $expand=Builder/City

                if (!selects.ExpansionIsValid(path))/*If selects are defined then check to make sure the expansion is one of them.*/
                    return listOfExpansionLists;

                Type currentParentType = parentType.GetCurrentType();
                Type memberType = currentParentType.GetMemberInfo(path).GetMemberType();
                Type elementType = memberType.GetCurrentType();

                ODataExpansionOptions exp = new ODataExpansionOptions
                {
                    MemberType = memberType,
                    ParentType = currentParentType,
                    MemberName = path,
                    FilterOptions = GetFilter(),
                    QueryOptions = GetQuery(),
                    Selects = next.SelectAndExpand.GetSelects()
                };

                List<List<ODataExpansionOptions>> navigationItems = next.GetNestedExpansions(elementType).Select
                (
                    expansions =>
                    {
                        expansions.Insert(0, exp);
                        return expansions;
                    }
                ).ToList();

                if (navigationItems.Any())
                    listOfExpansionLists.AddRange(navigationItems);
                else
                    listOfExpansionLists.Add(new List<ODataExpansionOptions> { exp });

                return listOfExpansionLists;

                FilterOptions GetFilter()
                    => HasFilter()
                        ? new FilterOptions(next.FilterOption)
                        : null;

                QueryOptions GetQuery()
                    => HasQuery()
                        ? new QueryOptions(next.OrderByOption, (int?)next.SkipOption, (int?)next.TopOption)
                        : null;

                bool HasFilter()
                    => memberType.IsList() && next.FilterOption != null;

                bool HasQuery()
                    => memberType.IsList() && (next.OrderByOption != null || next.SkipOption.HasValue || next.TopOption.HasValue);
            });
        }

        public static LambdaExpression GetFilterExpression(this FilterClause filterClause, Type type)
        {
            var parameters = new Dictionary<string, ParameterExpression>();

            return new FilterHelper
            (
                parameters,
                type
            )
            .GetFilterPart(filterClause.Expression)
            .GetFilter(type, parameters, filterClause.RangeVariable.Name);
        }

        private static Expression Unquote(this Expression exp)
            => exp.NodeType == ExpressionType.Quote
                ? ((UnaryExpression)exp).Operand.Unquote()
                : exp;

        /// <summary>
        /// Creates a list of navigation expressions from the list of period delimited navigation properties.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static IEnumerable<Expression<Func<TSource, object>>> BuildIncludes<TSource>(this IEnumerable<string> includes)
            where TSource : class
            => includes.Select(include => BuildSelectorExpression<TSource>(include)).ToList();

        private static Expression<Func<TSource, object>> BuildSelectorExpression<TSource>(string fullName, string parameterName = "i")
        {
            ParameterExpression param = Expression.Parameter(typeof(TSource), parameterName);

            return (Expression<Func<TSource, object>>)Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(new[] { typeof(TSource), typeof(object) }),
                param.BuildSelectorExpression(fullName, param.Name),
                param
            );
        }

        internal static IQueryable<TModel> UpdateQueryableExpression<TModel>(this IQueryable<TModel> query, List<List<ODataExpansionOptions>> expansions)
        {
            List<List<ODataExpansionOptions>> filters = GetFilters();
            List<List<ODataExpansionOptions>> methods = GetQueryMethods();

            if (!filters.Any() && !methods.Any())
                return query;

            Expression expression = query.Expression;

            if (methods.Any())
                expression = UpdateProjectionMethodExpression(expression);

            if (filters.Any())//do filter last so it runs before a Skip or Take call.
                expression = UpdateProjectionFilterExpression(expression);

            return query.Provider.CreateQuery<TModel>(expression);

            Expression UpdateProjectionFilterExpression(Expression projectionExpression)
            {
                filters.ForEach
                (
                    filterList => projectionExpression = ChildCollectionFilterUpdater.UpdaterExpansion
                    (
                        projectionExpression,
                        filterList
                    )
                );

                return projectionExpression;
            }

            Expression UpdateProjectionMethodExpression(Expression projectionExpression)
            {
                methods.ForEach
                (
                    methodList => projectionExpression = ChildCollectionOrderByUpdater.UpdaterExpansion
                    (
                        projectionExpression,
                        methodList
                    )
                );

                return projectionExpression;
            }

            List<List<ODataExpansionOptions>> GetFilters()
                => expansions.Aggregate(new List<List<ODataExpansionOptions>>(), (listOfLists, nextList) =>
                {
                    var filterNextList = nextList.Aggregate(new List<ODataExpansionOptions>(), (list, next) =>
                    {
                        if (next.FilterOptions != null)
                        {
                            list = list.ConvertAll
                            (
                                exp => new ODataExpansionOptions
                                {
                                    MemberName = exp.MemberName,
                                    MemberType = exp.MemberType,
                                    ParentType = exp.ParentType,
                                }
                            );//new list removing filter

                            list.Add
                            (
                                new ODataExpansionOptions
                                {
                                    MemberName = next.MemberName,
                                    MemberType = next.MemberType,
                                    ParentType = next.ParentType,
                                    FilterOptions = new FilterOptions(next.FilterOptions.FilterClause)
                                }
                            );//add expansion with filter

                            listOfLists.Add(list.ToList()); //Add the whole list to the list of filter lists
                                                            //Only the last item in each list has a filter
                                                            //Filters for parent expansions exist in their own lists
                            return list;
                        }

                        list.Add(next);

                        return list;
                    });

                    return listOfLists;
                });

            List<List<ODataExpansionOptions>> GetQueryMethods()
                => expansions.Aggregate(new List<List<ODataExpansionOptions>>(), (listOfLists, nextList) =>
                {
                    var filterNextList = nextList.Aggregate(new List<ODataExpansionOptions>(), (list, next) =>
                    {
                        if (next.QueryOptions != null)
                        {
                            list = list.ConvertAll
                            (
                                exp => new ODataExpansionOptions
                                {
                                    MemberName = exp.MemberName,
                                    MemberType = exp.MemberType,
                                    ParentType = exp.ParentType,
                                }
                            );//new list removing query options

                            list.Add
                            (
                                new ODataExpansionOptions
                                {
                                    MemberName = next.MemberName,
                                    MemberType = next.MemberType,
                                    ParentType = next.ParentType,
                                    QueryOptions = new QueryOptions(next.QueryOptions.OrderByClause, next.QueryOptions.Skip, next.QueryOptions.Top)
                                }
                            );//add expansion with query options

                            listOfLists.Add(list.ToList()); //Add the whole list to the list of query method lists
                                                            //Only the last item in each list has a query method
                                                            //Query methods for parent expansions exist in their own lists
                            return list;
                        }

                        list.Add(next);

                        return list;
                    });

                    return listOfLists;
                });
        }
    }

    public class ODataExpansionOptions : Expansion
    {
        public QueryOptions QueryOptions { get; set; }
        public FilterOptions FilterOptions { get; set; }
    }

    public class QueryOptions
    {
        public QueryOptions(OrderByClause orderByClause, int? skip, int? top)
        {
            OrderByClause = orderByClause;
            Skip = skip;
            Top = top;
        }

        public OrderByClause OrderByClause { get; set; }
        public int? Skip { get; set; }
        public int? Top { get; set; }
    }

    public class FilterOptions
    {
        public FilterOptions(FilterClause filterClause)
        {
            FilterClause = filterClause;
        }

        public FilterClause FilterClause { get; set; }
    }
}
