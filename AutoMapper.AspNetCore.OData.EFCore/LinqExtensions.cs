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

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(GetOrderByMethod(options, param), param);
        }

        /// <summary>
        /// Get OrderBy Method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression GetOrderByMethod<T>(this ODataQueryOptions<T> options, Expression expression)
        {
            if (options.OrderBy == null && options.Top == null)
                return null;

            if (options.OrderBy == null)
            {
                return Expression.Call
                (
                    typeof(Queryable),
                    "Take",
                    new[] { typeof(T) }, expression, Expression.Constant(options.Top.Value)
                );
            }

            return options.OrderBy.OrderByNodes.Aggregate(null, (Expression mce, OrderByNode orderByNode) =>
            {
                OrderByPropertyNode propertyNode = (OrderByPropertyNode)orderByNode;
                return mce == null
                    ? expression.GetOrderByCall(propertyNode.GetPropertyPath(), orderByNode.Direction)
                    : mce.GetThenByCall(propertyNode.GetPropertyPath(), orderByNode.Direction);
            })
            .GetSkipCall(options.Skip)
            .GetTakeCall(options.Top);
        }

        private static string GetPropertyPath(this OrderByPropertyNode propertyNode)
        {
            return GetPropertyPath((SingleValuePropertyAccessNode)propertyNode.OrderByClause.Expression);
            string GetPropertyPath(SingleValuePropertyAccessNode singleValuePropertyAccess)
            {
                switch (singleValuePropertyAccess.Source)
                {
                    case SingleNavigationNode navigationNode:
                        return $"{string.Join(".", navigationNode.BindingPath.PathSegments)}.{propertyNode.Property.Name}";
                    default:
                        return propertyNode.Property.Name;
                }
            }
        }

        public static Expression GetSkipCall(this Expression expression, SkipQueryOption skip)
        {
            if (skip == null) return expression;

            return Expression.Call
            (
                typeof(Queryable),
                "Skip",
                new[] { expression.GetUnderlyingElementType() },
                expression,
                Expression.Constant(skip.Value)
            );
        }

        public static Expression GetTakeCall(this Expression expression, TopQueryOption top)
        {
            if (top == null) return expression;

            return Expression.Call
            (
                typeof(Queryable),
                "Take",
                new[] { expression.GetUnderlyingElementType() },
                expression,
                Expression.Constant(top.Value)
            );
        }

        public static Expression GetOrderByCall(this Expression expression, string memberFullName, OrderByDirection sortDirection, string selectorParameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            MemberInfo memberInfo = sourceType.GetMemberInfoFromFullName(memberFullName);
            return Expression.Call
            (
                typeof(Queryable),
                sortDirection == OrderByDirection.Ascending ? "OrderBy" : "OrderByDescending",
                new Type[] { sourceType, memberInfo.GetMemberType() },
                expression,
                memberFullName.GetTypedSelector(sourceType, selectorParameterName)
            );
        }

        public static Expression GetThenByCall(this Expression expression, string memberFullName, OrderByDirection sortDirection, string selectorParameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            MemberInfo memberInfo = sourceType.GetMemberInfoFromFullName(memberFullName);
            return Expression.Call
            (
                typeof(Queryable),
                sortDirection == OrderByDirection.Ascending ? "ThenBy" : "ThenByDescending",
                new Type[] { sourceType, memberInfo.GetMemberType() },
                expression,
                memberFullName.GetTypedSelector(sourceType, selectorParameterName)
            );
        }

        public static Type GetUnderlyingElementType(this Expression expression)
            => expression.Type.GetUnderlyingElementType();

        public static MemberInfo GetMemberInfoFromFullName(this Type type, string propertyFullName)
        {
            int indexOfSeparator = propertyFullName.IndexOf('.');
            if (indexOfSeparator < 0)
            {
                return type.GetMemberInfo(propertyFullName);
            }

            string propertyName = propertyFullName.Substring(0, indexOfSeparator);
            string childFullName = propertyFullName.Substring(indexOfSeparator + 1);

            return GetMemberInfoFromFullName(type.GetMemberInfo(propertyName).GetMemberType(), childFullName);
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

            return clause.SelectExpandClause.SelectedItems
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
            if (clause == null)
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
            => (Expression<Func<TSource, object>>)BuildSelectorExpression(typeof(TSource), fullName, parameterName);

        private static LambdaExpression BuildSelectorExpression(Type type, string fullName, string parameterName = "i")
        {
            ParameterExpression param = Expression.Parameter(type, parameterName);
            string[] parts = fullName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Type parentType = type;
            Expression parent = param;

            for (int i = 0; i < parts.Length; i++)
            {
                if (parentType.IsList())
                {
                    parent = GetSelectExpression(parts.Skip(i), parent, parentType.GenericTypeArguments[0], parameterName);//parentType is the underlying type of the member since it is an IEnumerable<T>
                    return Expression.Lambda
                    (
                        typeof(Func<,>).MakeGenericType(new[] { type, typeof(object) }),
                        parent,
                        param
                    );
                }
                else
                {
                    MemberInfo mInfo = parentType.GetMemberInfo(parts[i]);
                    parent = Expression.MakeMemberAccess(parent, mInfo);

                    parentType = mInfo.GetMemberType();
                }
            }

            if (parent.Type.IsValueType)//Convert value type expressions to object expressions otherwise
                parent = Expression.Convert(parent, typeof(object));//Expression.Lambda below will throw an exception for value types

            return Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(new[] { type, typeof(object) }),
                parent,
                param
            );
        }

        private static string ChildParameterName(this string currentParameterName)
        {
            string lastChar = currentParameterName.Substring(currentParameterName.Length - 1);
            if (short.TryParse(lastChar, out short lastCharShort))
            {
                return string.Concat(currentParameterName.Substring(0, currentParameterName.Length - 1), (lastCharShort++).ToString(CultureInfo.CurrentCulture));
            }
            else
            {
                return currentParameterName += "0";
            }
        }

        private static Expression GetSelectExpression(IEnumerable<string> parts, Expression parent, Type underlyingType, string parameterName)//underlying type because paranet is a collection
            => Expression.Call
            (
                typeof(Enumerable),//This is an Enumerable (not Queryable) select.  We are selecting includes for a member who is a collection
                "Select",
                new Type[] { underlyingType, typeof(object) },
                parent,
                BuildSelectorExpression(underlyingType, string.Join(".", parts), parameterName.ChildParameterName())//Join the remaining parts to create a full name
            );
    }
}
