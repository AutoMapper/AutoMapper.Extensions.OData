using AutoMapper.AspNet.OData.Visitors;
using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Replace source parameter expression to target expression.
        /// </summary>
        /// <param name="expression">Original expression for replace.</param>
        /// <param name="source">Parameter expression from source expression.</param>
        /// <param name="target">Target expression for replace.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public static Expression ReplaceParameter(this Expression expression,
            ParameterExpression source,
            Expression target)
        {
            return new ParameterReplacer(source, target).Visit(expression);
        }
        
        /// <summary>
        /// Returns a lambda expression representing the filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterOption"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> ToFilterExpression<T>(this FilterQueryOption filterOption, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default, TimeZoneInfo timeZone = null)
        {
            if (filterOption == null)
                return null;

            IQueryable queryable = Enumerable.Empty<T>().AsQueryable();

            queryable = filterOption.ApplyTo(queryable, new ODataQuerySettings() { HandleNullPropagation = handleNullPropagation, TimeZone = timeZone });

            MethodCallExpression whereMethodCallExpression = (MethodCallExpression)queryable.Expression;

            return (Expression<Func<T, bool>>)(whereMethodCallExpression.Arguments[1].Unquote() as LambdaExpression);
        }

        /// <summary>
        /// Returns a lambda expression representing the filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterOption"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> ToSearchExpression<T>(this SearchQueryOption filterOption, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default, TimeZoneInfo timeZone = null)
        {
            if (filterOption == null)
                return null;

            IQueryable queryable = Enumerable.Empty<T>().AsQueryable();
            queryable = filterOption.ApplyTo(queryable, new ODataQuerySettings() { HandleNullPropagation = handleNullPropagation, TimeZone = timeZone });

            MethodCallExpression whereMethodCallExpression = (MethodCallExpression)queryable.Expression;

            return (Expression<Func<T, bool>>)(whereMethodCallExpression.Arguments[1].Unquote() as LambdaExpression);
        }
        
        public static Expression<Func<T, bool>> ToFilterExpression<T>(this ODataQueryOptions<T> options,
            HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default,
            TimeZoneInfo timeZone = null)
        {
            if (options is null || options.Filter is null && options.Search is null)
            {
                return null;
            }

            var parameter = Expression.Parameter(typeof(T), "$it");
            
            Expression filterExpression = null;
            if (options.Filter is not null)
            {
                var raw = options.Filter.ToFilterExpression<T>(handleNullPropagation, timeZone);
                filterExpression = raw.Body.ReplaceParameter(raw.Parameters[0], parameter);
            }
            
            Expression searchExpression = null;
            if (options.Search is not null)
            {
                var raw = options.Search.ToSearchExpression<T>(handleNullPropagation, timeZone);
                searchExpression = raw.Body.ReplaceParameter(raw.Parameters[0], parameter);
            }

            Expression finalExpression = null;
            if (filterExpression is not null && searchExpression is not null)
            {
                finalExpression = Expression.AndAlso(searchExpression, filterExpression);
            }

            finalExpression ??= filterExpression ?? searchExpression;
            return Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
        }

        /// <summary>
        /// Returns a lambda expresion for order and paging expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <param name="oDataSettings"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> GetQueryableExpression<T>(this ODataQueryOptions<T> options, ODataSettings oDataSettings = null)
        {
            if (NoQueryableMethod(options, oDataSettings))
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>
            (
                param.GetOrderByMethod(options, oDataSettings), param
            );
        }

        public static Expression GetOrderByMethod<T>(this Expression expression,
            ODataQueryOptions<T> options, ODataSettings oDataSettings = null)
        {            
            if (NoQueryableMethod(options, oDataSettings))
                return null;

            return expression.GetQueryableMethod
            (
                options.Context,
                options.OrderBy?.OrderByClause,
                typeof(T),
                options.Skip?.Value,
                GetPageSize()
            );

            int? GetPageSize()
            {
                if (oDataSettings?.PageSize == null && options.Top == null)
                    return null;

                if (options.Top == null)
                    return oDataSettings.PageSize;
                else if (oDataSettings?.PageSize == null)
                    return options.Top.Value;

                return options.Top.Value < oDataSettings.PageSize
                    ? options.Top.Value
                    : oDataSettings.PageSize;
            }
        }        

        public static Expression GetQueryableMethod(this Expression expression,
            ODataQueryContext context, OrderByClause orderByClause, Type type, int? skip, int? top)
        {            
            if (orderByClause is null && skip is null && top is null)
                return null;

            if (orderByClause is null && (skip is not null || top is not null))
            {                       
                var orderBySettings = context.FindSortableProperties(type);

                if (orderBySettings is null)
                    return null;

               return expression
                   .GetDefaultOrderByCall(orderBySettings)
                   .GetSkipCall(skip)
                   .GetTakeCall(top);
            }

            return expression
                .GetOrderByCall(orderByClause, context)
                .GetSkipCall(skip)
                .GetTakeCall(top);
        }

        private static bool NoQueryableMethod(ODataQueryOptions options, ODataSettings oDataSettings)
            => options.OrderBy is null
            && options.Top is null
            && options.Skip is null
            && oDataSettings?.PageSize is null;


        private static Expression GetDefaultThenByCall(this Expression expression, OrderBySetting settings)
        {
            return settings.ThenBy is null
                ? GetMethodCall()
                : GetMethodCall().GetDefaultThenByCall(settings.ThenBy);

            Expression GetMethodCall() =>
                expression.GetOrderByCall(settings.Name, nameof(Queryable.ThenBy));
        }

        private static Expression GetDefaultOrderByCall(this Expression expression, OrderBySetting settings)
        {
            return settings.ThenBy is null 
                ? GetMethodCall()
                : GetMethodCall().GetDefaultThenByCall(settings.ThenBy);

            Expression GetMethodCall() => 
                expression.GetOrderByCall(settings.Name, nameof(Queryable.OrderBy));            
        }

        private static Expression GetOrderByCall(this Expression expression, OrderByClause orderByClause, ODataQueryContext context)
        {
            const string OrderBy = "OrderBy";
            const string OrderByDescending = "OrderByDescending";

            return orderByClause.ThenBy == null
                ? GetMethodCall()
                : GetMethodCall().GetThenByCall(orderByClause.ThenBy, context);

            Expression GetMethodCall()
            {
                SingleValueNode orderByNode = orderByClause.Expression;
                switch (orderByNode)
                {
                    case CountNode countNode:
                        return expression.GetOrderByCountCall
                        (
                            countNode,
                            orderByClause.Direction == OrderByDirection.Ascending
                                ? OrderBy
                                : OrderByDescending,
                            context,
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

        private static Expression GetThenByCall(this Expression expression, OrderByClause orderByClause, ODataQueryContext context)
        {
            const string ThenBy = "ThenBy";
            const string ThenByDescending = "ThenByDescending";

            return orderByClause.ThenBy == null
                ? GetMethodCall()
                : GetMethodCall().GetThenByCall(orderByClause.ThenBy, context);

            Expression GetMethodCall()
            {
                return orderByClause.Expression switch
                {
                    CountNode countNode => expression.GetOrderByCountCall
                    (
                        countNode,
                        orderByClause.Direction == OrderByDirection.Ascending
                            ? ThenBy
                            : ThenByDescending, 
                        context
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
                    throw new ArgumentNullException(nameof(countNode));
                default:
                    throw new ArgumentOutOfRangeException(nameof(countNode));
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

        [Obsolete("\"Expression GetOrderByCountCall(this Expression expression, CountNode countNode, string methodName, ODataQueryContext context, string selectorParameterName = \"a\")\"")]
        public static Expression GetOrderByCountCall(this Expression expression, CountNode countNode, string methodName, string selectorParameterName = "a")
        {
            return expression.GetOrderByCountCall(countNode, methodName, null, selectorParameterName);
        }

        public static Expression GetOrderByCountCall(this Expression expression, CountNode countNode, string methodName, ODataQueryContext context, string selectorParameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            ParameterExpression param = Expression.Parameter(sourceType, selectorParameterName);

            Expression countSelector;

            if (countNode.FilterClause is not null)
            {
                string memberFullName = countNode.GetPropertyPath();
                Type filterType = sourceType.GetMemberInfoFromFullName(memberFullName).GetMemberType().GetUnderlyingElementType();
                LambdaExpression filterExpression = countNode.FilterClause.GetFilterExpression(filterType, context);
                countSelector = param.MakeSelector(memberFullName).GetCountCall(filterExpression);
            }
            else
            {
                countSelector = param.MakeSelector(countNode.GetPropertyPath()).GetCountCall();
            }
            
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

            return clause.SelectExpandClause.SelectedItems.GetExpansions(parentType);
        }

        private static List<List<ODataExpansionOptions>> GetNestedExpansions(this ExpandedNavigationSelectItem node, Type type)
        {
            if (node == null)
                return new List<List<ODataExpansionOptions>>();

            return node.SelectAndExpand.SelectedItems.GetExpansions(type);
        }

        private static List<List<ODataExpansionOptions>> GetExpansions(this IEnumerable<SelectItem> selectedItems, Type parentType)
        {
            if (selectedItems == null)
                return new List<List<ODataExpansionOptions>>();

            return selectedItems.OfType<ExpandedNavigationSelectItem>().Aggregate(new List<List<ODataExpansionOptions>>(), (listOfExpansionLists, next) =>
            {
                string path = next.PathToNavigationProperty.FirstSegment.Identifier;//Only first segment is necessary because of the new syntax $expand=Builder($expand=City) vs $expand=Builder/City

                Type currentParentType = parentType.GetCurrentType();
                Type memberType = currentParentType.GetMemberInfo(path).GetMemberType();
                Type elementType = memberType.GetCurrentType();

                ODataExpansionOptions exp = new()
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

        [Obsolete("\"LambdaExpression GetFilterExpression(this FilterClause filterClause, Type type, ODataQueryContext context)\"")]
        public static LambdaExpression GetFilterExpression(this FilterClause filterClause, Type type) 
            => filterClause.GetFilterExpression(type, null);

        public static LambdaExpression GetFilterExpression(this FilterClause filterClause, Type type, ODataQueryContext context)
        {
            var parameters = new Dictionary<string, ParameterExpression>();

            return new FilterHelper
            (
                parameters,
                type,
                context
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

        internal static IQueryable<TModel> UpdateQueryableExpression<TModel>(
            this IQueryable<TModel> query, List<List<ODataExpansionOptions>> expansions, ODataQueryContext context)
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
                        filterList,
                        context
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
                        methodList,
                        context
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
