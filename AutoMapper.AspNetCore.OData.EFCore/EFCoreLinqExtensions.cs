using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    public static class EFCoreLinqExtensions
    {
        /// <summary>
        /// Creates a list of Include expressions from a list of properties. Each property may include child and granchild properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static ICollection<Expression<Func<IQueryable<T>, IIncludableQueryable<T, object>>>> BuildIncludesExpressionCollection<T>(this IEnumerable<string> includes) where T : class
            => (includes == null || includes.Count() == 0)
                ? null
                : includes.Select(i => i.BuildIncludeExpression<T>()).ToList();

        /// <summary>
        /// Creates an Include expression from a property. The property may include child and granchild properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IIncludableQueryable<T, object>>> BuildIncludeExpression<T>(this string include) where T : class
        {
            if (string.IsNullOrEmpty(include)) return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetInclude<T>(include);
            return Expression.Lambda<Func<IQueryable<T>, IIncludableQueryable<T, object>>>(mce, param);
        }

        /// <summary>
        /// Creates an include method call expression to be invoked on an expression e.g. (parameter, member, method call) of type IQueryable<T>.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="includes"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetInclude<TSource>(this Expression expression, string include)
        {
            if (string.IsNullOrEmpty(include)) return null;
            ICollection<string> includes = include.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Type parentType = typeof(TSource);

            return includes.Aggregate(null, (MethodCallExpression mce, string next) =>
            {
                LambdaExpression selectorExpression = next.GetTypedSelector(parentType);
                MemberInfo mInfo = parentType.GetMemberInfo(next);

                mce = mce == null
                        //The Include espression takes two arguments.  The parameter (object being extended by the helper method) and the lambda expression for the property selector
                        ? Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include", new Type[] { parentType, mInfo.GetMemberType() }, expression, selectorExpression)
                        //The ThenInclude espression takes two arguments.  The resulting method call expression from Include and the lambda expression for the property selector
                        : Expression.Call(typeof(EntityFrameworkQueryableExtensions), "ThenInclude", new Type[] { typeof(TSource), parentType, mInfo.GetMemberType() }, mce, selectorExpression);

                parentType = mInfo.GetMemberType().GetCurrentType();//new previous property to include members from.

                return mce;
            });
        }
    }
}
