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
        public static MethodCallExpression GetOrderByMethod<T>(this ODataQueryOptions<T> options, Expression expression)
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

            IQueryable queryable = Enumerable.Empty<T>().AsQueryable();
            queryable = options.OrderBy.ApplyTo(queryable, new ODataQuerySettings());
            MethodCallExpression mce = (MethodCallExpression)queryable.Expression;

            mce = Expression.Call(typeof(Queryable), mce.Method.Name, new Type[] { typeof(T), mce.Arguments[1].GetReturnType() }, expression, mce.Arguments[1]);

            if (options.Skip != null)
                mce = Expression.Call(typeof(Queryable), "Skip", new[] { typeof(T) }, mce, Expression.Constant(options.Skip.Value));
            if (options.Top != null)
                mce = Expression.Call(typeof(Queryable), "Take", new[] { typeof(T) }, mce, Expression.Constant(options.Top.Value));

            return mce;
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

        private static Type GetReturnType(this Expression exp)
            => exp.NodeType == ExpressionType.Quote
                ? (exp.Unquote() as LambdaExpression).ReturnType
                : (exp as LambdaExpression).ReturnType;

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
