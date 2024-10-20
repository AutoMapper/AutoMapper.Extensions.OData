using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LogicBuilder.Expressions.Utils;

namespace AutoMapper.AspNet.OData
{
    public static class QueryableExtensions
    {
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>(
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
                querySettings?.ODataSettings?.TimeZone,
                querySettings?.ODataSettings?.EnableConstantParameterization ?? true);

            query.ApplyOptions(mapper, filter, options, querySettings);
            return query.Get
            (
                mapper,
                filter,
                options.GetQueryableExpression(querySettings?.ODataSettings),
                options.SelectExpand.GetIncludes().BuildIncludesExpressionCollection<TModel>()?.ToList()
            );
        }

        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {            
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>(
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
                querySettings?.ODataSettings?.TimeZone,
                querySettings?.ODataSettings?.EnableConstantParameterization ?? true);
            await query.ApplyOptionsAsync(mapper, filter, options, querySettings);
            return await query.GetAsync
            (
                mapper,
                filter,
                options.GetQueryableExpression(querySettings?.ODataSettings),
                options.SelectExpand.GetIncludes().BuildIncludesExpressionCollection<TModel>()?.ToList(),
                querySettings?.AsyncSettings
            );
        }

        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>(
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
                querySettings?.ODataSettings?.TimeZone,
                querySettings?.ODataSettings?.EnableConstantParameterization ?? true);
                
            await query.ApplyOptionsAsync(mapper, filter, options, querySettings);
            return query.GetQueryable(mapper, options, querySettings, filter);
        }

        public static IQueryable<TModel> GetQuery<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>(
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
                querySettings?.ODataSettings?.TimeZone,
                querySettings?.ODataSettings?.EnableConstantParameterization ?? true);
            query.ApplyOptions(mapper, filter, options, querySettings);
            return query.GetQueryable(mapper, options, querySettings, filter);
        }

        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            => mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>
            (
                query.GetDataQuery(mapper, filter, queryFunc, includeProperties).ToList()
            ).ToList();


        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null,
            AsyncSettings asyncSettings = null)
            => mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>
            (
                await query
                    .GetDataQuery(mapper, filter, queryFunc, includeProperties)
                    .ToListAsync
                    (
                        asyncSettings?.CancellationToken ?? default
                    )
            ).ToList();

        public static async Task ApplyOptionsAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, Expression<Func<TModel, bool>> filter, ODataQueryOptions<TModel> options, QuerySettings querySettings)
        {
            ApplyOptions(options, querySettings);
            if (options.Count?.Value == true)
                options.AddCountOptionsResult(await query.QueryLongCountAsync(mapper, filter, querySettings?.AsyncSettings?.CancellationToken ?? default));
        }

        private static IQueryable<TData> GetDataQuery<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            if (filter != null)
                query = query.Where(f);

            if (includes != null)
                query = includes.Select(i => i.Compile()).Aggregate(query, (q, next) => q = next(q));

            return mappedQueryFunc != null ? mappedQueryFunc(query) : query;
        }

        private static IQueryable<TModel> GetQueryable<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            ODataQueryOptions<TModel> options,
            QuerySettings querySettings,
            Expression<Func<TModel, bool>> filter)
            where TModel : class
        {
            
            var expansions = options.SelectExpand.GetExpansions(typeof(TModel));
            
            var includeProperties = expansions
                .Select(list => new List<Expansion>(list))
                .BuildIncludes<TModel>(options.SelectExpand.GetSelects());

            var includeLiteralLists = expansions
                .Select(list => new List<Expansion>(list))
                .BuildWithLiteralLists<TModel>(options.SelectExpand.GetSelects());

            var includes = includeProperties
                .UnionBy(includeLiteralLists, e => e.Body.ToString())
                .ToList();
            
            return query.GetQuery
            (
                mapper,
                filter,
                options.GetQueryableExpression(querySettings?.ODataSettings),
                includes,
                querySettings?.ProjectionSettings
            ).UpdateQueryableExpression(expansions, options.Context);
        }

        private static IQueryable<TModel> GetQuery<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
            ProjectionSettings projectionSettings = null)
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            if (filter != null)
                query = query.Where(f);

            return mappedQueryFunc != null
                    ? mapper.ProjectTo(mappedQueryFunc(query), projectionSettings?.Parameters, GetIncludes())
                    : mapper.ProjectTo(query, projectionSettings?.Parameters, GetIncludes());

            Expression<Func<TModel, object>>[] GetIncludes() => includeProperties?.ToArray() ?? new Expression<Func<TModel, object>>[] { };
        }

        private static void ApplyOptions<TModel, TData>(this IQueryable<TData> query, IMapper mapper, Expression<Func<TModel, bool>> filter, ODataQueryOptions<TModel> options, QuerySettings querySettings)
        {
            ApplyOptions(options, querySettings);
            if (options.Count?.Value == true)
                options.AddCountOptionsResult(query.QueryLongCount(mapper, filter));
        }

        private static void ApplyOptions<TModel>(ODataQueryOptions<TModel> options, QuerySettings querySettings)
        {
            options.AddExpandOptionsResult();
            if (querySettings?.ODataSettings?.PageSize.HasValue == true)
                options.AddNextLinkOptionsResult(querySettings.ODataSettings.PageSize.Value);
        }

        private static async Task<long> QueryLongCountAsync<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            Expression<Func<TModel, bool>> modelFilter,
            CancellationToken cancellationToken)
            => modelFilter == null
                ? await query.LongCountAsync(cancellationToken)
                : await query.LongCountAsync
                (
                    mapper.MapExpression<Expression<Func<TData, bool>>>(modelFilter),
                    cancellationToken
                );

        private static long QueryLongCount<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            Expression<Func<TModel, bool>> modelFilter)
            => modelFilter == null
                ? query.LongCount()
                : query.LongCount
                (
                    mapper.MapExpression<Expression<Func<TData, bool>>>(modelFilter)
                );
        
        private static ICollection<Expression<Func<TSource, object>>> BuildWithLiteralLists<TSource>(this IEnumerable<List<Expansion>> includes, List<string> selects)
            where TSource : class
        {
            return GetAllExpansions(new List<LambdaExpression>());
        
            List<Expression<Func<TSource, object>>> GetAllExpansions(List<LambdaExpression> valueMemberSelectors)
            {
                string parameterName = "i";
                ParameterExpression param = Expression.Parameter(typeof(TSource), parameterName);
        
                valueMemberSelectors.AddSelectors(selects, param, param);
        
                return includes
                    .Select(include => BuildSelectorExpression<TSource>(include, valueMemberSelectors, parameterName))
                    .Concat(valueMemberSelectors.Select(selector => (Expression<Func<TSource, object>>)selector))
                    .ToList();
            }
        }
        
        private static Expression<Func<TSource, object>> BuildSelectorExpression<TSource>(List<Expansion> fullName, List<LambdaExpression> valueMemberSelectors, string parameterName = "i")
        {
            ParameterExpression param = Expression.Parameter(typeof(TSource), parameterName);

            return (Expression<Func<TSource, object>>)Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(new[] { param.Type, typeof(object) }),
                BuildSelectorExpression(param, fullName, valueMemberSelectors, parameterName),
                param
            );
        }
        
        // e.g. /opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))
        private static Expression BuildSelectorExpression(Expression sourceExpression, List<Expansion> parts, List<LambdaExpression> valueMemberSelectors, string parameterName = "i")
        {
            Expression parent = sourceExpression;

            //Arguments to create a nested expression when the parent expansion is a collection
            //See AddChildSeelctors() below
            List<LambdaExpression> childValueMemberSelectors = new List<LambdaExpression>();

            for (int i = 0; i < parts.Count; i++)
            {
                if (parent.Type.IsList())
                {
                    Expression selectExpression = GetSelectExpression
                    (
                        parts.Skip(i),
                        parent,
                        childValueMemberSelectors,
                        parameterName
                    );

                    AddChildSeelctors();

                    return selectExpression;
                }
                else
                {
                    parent = Expression.MakeMemberAccess(parent, parent.Type.GetMemberInfo(parts[i].MemberName));

                    if (parent.Type.IsList())
                    {
                        ParameterExpression childParam = Expression.Parameter(parent.GetUnderlyingElementType(), parameterName.ChildParameterName());
                        //selectors from an underlying list element must be added here.
                        childValueMemberSelectors.AddSelectors
                        (
                            parts[i].Selects,
                            childParam,
                            childParam
                        );
                    }
                    else
                    {
                        valueMemberSelectors.AddSelectors(parts[i].Selects, Expression.Parameter(sourceExpression.Type, parameterName), parent);
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
                childValueMemberSelectors.ForEach(selector =>
                {
                    valueMemberSelectors.Add(Expression.Lambda
                    (
                        typeof(Func<,>).MakeGenericType(new[] { sourceExpression.Type, typeof(object) }),
                        Expression.Call
                        (
                            typeof(Enumerable),
                            "Select",
                            new Type[] { parent.GetUnderlyingElementType(), typeof(object) },
                            parent,
                            selector
                        ),
                        Expression.Parameter(sourceExpression.Type, parameterName)
                    ));
                });
            }
        }
        
        private static Expression GetSelectExpression(IEnumerable<Expansion> expansions, Expression parent, List<LambdaExpression> valueMemberSelectors, string parameterName)
        {
            ParameterExpression parameter = Expression.Parameter(parent.GetUnderlyingElementType(), parameterName.ChildParameterName());
            Expression selectorBody = BuildSelectorExpression(parameter, expansions.ToList(), valueMemberSelectors, parameter.Name);
            return Expression.Call
            (
                typeof(Enumerable),
                "Select",
                new Type[] { parameter.Type, selectorBody.Type },
                parent,
                Expression.Lambda
                (
                    typeof(Func<,>).MakeGenericType(new[] { parameter.Type, selectorBody.Type }),
                    selectorBody,
                    parameter
                )
            );
        }

        private static string ChildParameterName(this string currentParameterName)
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
        
        private static void AddSelectors(this List<LambdaExpression> valueMemberSelectors, List<string> selects, ParameterExpression param, Expression parentBody)
        {
            if (parentBody.Type.IsList() || parentBody.Type.IsLiteralType())
                return;

            valueMemberSelectors.AddRange
            (
                parentBody.Type
                    .GetSelectedMembers(selects)
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
                            typeof(Func<,>).MakeGenericType(new[] { param.Type, typeof(object) }),
                            selector,
                            param
                        )
                    )
            );
        }
    }
}
