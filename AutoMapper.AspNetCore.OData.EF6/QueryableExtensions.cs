using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoMapper.AspNet.OData
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <param name="querySettings"></param>
        /// <returns></returns>
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>
            (
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.Default,
                enableConstantParameterization: querySettings?.ODataSettings?.EnableConstantParameterization ?? true
            );
            query.ApplyOptions(mapper, filter, options, querySettings);
            return query.Get
            (
                mapper,
                filter,
                options.GetQueryableExpression(querySettings?.ODataSettings),
                options.SelectExpand.GetIncludes().BuildIncludes<TModel>().ToList()
            );
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <param name="querySettings"></param>
        /// <returns></returns>
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>
            (
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.Default, 
                enableConstantParameterization: querySettings?.ODataSettings?.EnableConstantParameterization ?? true
            );
            await query.ApplyOptionsAsync(mapper, filter, options, querySettings);
            return await query.GetAsync
            (
                mapper, 
                filter,
                options.GetQueryableExpression(querySettings?.ODataSettings),
                options.SelectExpand.GetIncludes().BuildIncludes<TModel>().ToList(),
                querySettings?.AsyncSettings
            );
        }

        /// <summary>
        /// GetQueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <param name="querySettings"></param>
        /// <returns></returns>
        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>
            (
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False, 
                enableConstantParameterization: querySettings?.ODataSettings?.EnableConstantParameterization ?? true
            );
            await query.ApplyOptionsAsync(mapper, filter, options, querySettings);
            return query.GetQueryable(mapper, options, querySettings, filter);
        }

        /// <summary>
        /// GetQuery
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <param name="querySettings"></param>
        /// <returns></returns>
        public static IQueryable<TModel> GetQuery<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
        {
            Expression<Func<TModel, bool>> filter = options.ToFilterExpression<TModel>
            (
                querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
                enableConstantParameterization: querySettings?.ODataSettings?.EnableConstantParameterization ?? true
            );
            query.ApplyOptions(mapper, filter, options, querySettings);
            return query.GetQueryable(mapper, options, querySettings, filter);
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="filter"></param>
        /// <param name="queryFunc"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null) 
            => mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>
            (
                query.GetDataQuery(mapper, filter, queryFunc, includeProperties).ToList()
            ).ToList();

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="filter"></param>
        /// <param name="queryFunc"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
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

        private static IQueryable<TData> GetDataQuery<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null)
        {
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();
            ICollection<Expression<Func<TData, object>>> includes = mapper.MapIncludesList<Expression<Func<TData, object>>>(includeProperties);

            if (filter != null)
                query = query.Where(f);

            if (includes != null)
                query = includes.Aggregate(query, (q, next) => q.Include(next));

            return mappedQueryFunc != null ? mappedQueryFunc(query) : query;
        }

        private static IQueryable<TModel> GetQueryable<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            ODataQueryOptions<TModel> options,
            QuerySettings querySettings,
            Expression<Func<TModel, bool>> filter)
            where TModel : class
        {
            var expansions = options.SelectExpand.GetExpansions(typeof(TModel), options.Context.Model);

            return query.GetQuery
            (
                mapper,
                filter,
                options.GetQueryableExpression(querySettings?.ODataSettings),
                expansions
                    .Select(list => new List<Expansion>(list))
                    .BuildIncludes<TModel>(options.SelectExpand.GetSelects())
                    .ToList(),
                querySettings?.ProjectionSettings
            ).UpdateQueryableExpression(expansions, options.Context, mapper);
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

        private static async Task ApplyOptionsAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, Expression<Func<TModel, bool>> filter, ODataQueryOptions<TModel> options, QuerySettings querySettings)
        {
            ApplyOptions(options, querySettings);
            if (options.Count?.Value == true)
                options.AddCountOptionsResult(await query.QueryLongCountAsync(mapper, filter, querySettings?.AsyncSettings?.CancellationToken ?? default));
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
    }
}
