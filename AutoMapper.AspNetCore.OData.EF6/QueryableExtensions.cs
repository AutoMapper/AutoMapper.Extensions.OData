using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.AspNet.OData.Query;
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
        /// <param name="handleNullPropagation"></param>
        /// <returns></returns>
        [Obsolete("\"Use ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings)\"")]
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
            => Task.Run(async () => await query.GetAsync(mapper, options, handleNullPropagation)).GetAwaiter().GetResult();

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
            => Task.Run(async () => await query.GetAsync(mapper, options, querySettings)).GetAwaiter().GetResult();

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
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, 
            ODataQueryOptions<TModel> options, 
            QuerySettings querySettings = null,
            CancellationToken cancellationToken = default)
            where TModel : class
        {
            List<Expression<Func<TModel, object>>> includeExpressions = options.SelectExpand.GetIncludes().BuildIncludes<TModel>().ToList();
            Expression<Func<TModel, bool>> filter = options.Filter.ToFilterExpression<TModel>(querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.Default);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = options.GetQueryableExpression(querySettings?.ODataSettings);
            Expression<Func<IQueryable<TModel>, long>> countExpression = LinqExtensions.GetCountExpression<TModel>(filter);

            options.AddExpandOptionsResult();
            if (options.Count?.Value == true)
                options.AddCountOptionsResult<TModel, TData>(await query.QueryAsync(mapper, countExpression, cancellationToken: cancellationToken));

            if (querySettings?.ODataSettings?.PageSize.HasValue == true)
                options.AddNextLinkOptionsResult(querySettings.ODataSettings.PageSize.Value);

            ICollection<TModel> collection = await query.GetAsync(mapper, filter, queryableExpression, includeExpressions, cancellationToken);

            return collection;
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <param name="handleNullPropagation"></param>
        /// <returns></returns>
        [Obsolete("\"Use Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)\"")]
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, 
            ODataQueryOptions<TModel> options, 
            HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default,
            CancellationToken cancellationToken = default)
            where TModel : class
            => await query.GetAsync(mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = handleNullPropagation } }, cancellationToken);

        /// <summary>
        /// GetQueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <param name="handleNullPropagation"></param>
        /// <returns></returns>
        [Obsolete("\"Use Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)\"")]
        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, 
            ODataQueryOptions<TModel> options, 
            HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default,
            CancellationToken cancellationToken = default)
            where TModel : class
            => await query.GetQueryAsync(mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = handleNullPropagation } }, cancellationToken);

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
        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, 
            ODataQueryOptions<TModel> options, 
            QuerySettings querySettings = null,
            CancellationToken cancellationToken = default)
            where TModel : class
        {
            var expansions = options.SelectExpand.GetExpansions(typeof(TModel));
            List<Expression<Func<TModel, object>>> includeExpressions = expansions.Select(list => new List<Expansion>(list)).BuildIncludes<TModel>
            (
                options.SelectExpand.GetSelects()
            )
            .ToList();

            Expression<Func<TModel, bool>> filter = options.Filter.ToFilterExpression<TModel>(querySettings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.Default);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = options.GetQueryableExpression(querySettings?.ODataSettings);
            Expression<Func<IQueryable<TModel>, long>> countExpression = LinqExtensions.GetCountExpression<TModel>(filter);

            options.AddExpandOptionsResult();
            if (options.Count?.Value == true)
                options.AddCountOptionsResult<TModel, TData>(await query.QueryAsync(mapper, countExpression));

            if (querySettings?.ODataSettings?.PageSize.HasValue == true)
                options.AddNextLinkOptionsResult(querySettings.ODataSettings.PageSize.Value);

            IQueryable<TModel> queryable = await query.GetQueryAsync(mapper, filter, queryableExpression, includeExpressions, querySettings?.ProjectionSettings, cancellationToken);

            return queryable.UpdateQueryableExpression(expansions);
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
            => Task.Run(async () => await query.GetAsync(mapper, filter, queryFunc, includeProperties)).GetAwaiter().GetResult();

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
            CancellationToken cancellationToken = default)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();
            ICollection<Expression<Func<TData, object>>> includes = mapper.MapIncludesList<Expression<Func<TData, object>>>(includeProperties);

            if (filter != null)
                query = query.Where(f);

            if (includes != null)
                query = includes.Aggregate(query, (q, next) => q.Include(next));

            //Call the store
            ICollection<TData> result = mappedQueryFunc != null ? await mappedQueryFunc(query).ToListAsync(cancellationToken) : await query.ToListAsync(cancellationToken);

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(result).ToList();
        }

        /// <summary>
        /// GetQueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="filter"></param>
        /// <param name="queryFunc"></param>
        /// <param name="includeProperties"></param>
        /// <param name="projectionSettings"></param>
        /// <returns></returns>
        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
            ProjectionSettings projectionSettings = null,
            CancellationToken cancellationToken = default)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            if (filter != null)
                query = query.Where(f);

            return await Task.Run
            (
                () => mappedQueryFunc != null
                    ? mapper.ProjectTo(mappedQueryFunc(query), projectionSettings?.Parameters, GetIncludes())
                    : mapper.ProjectTo(query, projectionSettings?.Parameters, GetIncludes()),
                cancellationToken
            );

            Expression<Func<TModel, object>>[] GetIncludes() => includeProperties?.ToArray() ?? new Expression<Func<TModel, object>>[] { };
        }

        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TModelReturn"></typeparam>
        /// <typeparam name="TDataReturn"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="queryFunc"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public static async Task<TReturn> QueryAsync<TModel, TData, TModelReturn, TDataReturn, TReturn>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            Func<IQueryable<TData>, TDataReturn> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataReturn>>>(queryFunc).Compile();
            ICollection<Expression<Func<TData, object>>> includes = mapper.MapIncludesList<Expression<Func<TData, object>>>(includeProperties);

            if (includes != null)
                query = includes.Aggregate(query, (q, next) => q.Include(next));

            TDataReturn result = await Task.Run(() => mappedQueryFunc(query), cancellationToken);

            return typeof(TReturn) == typeof(TDataReturn) ? (TReturn)(object)result : mapper.Map<TDataReturn, TReturn>(result);
        }

        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TModelReturn"></typeparam>
        /// <typeparam name="TDataReturn"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="queryFunc"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public static async Task<TModelReturn> QueryAsync<TModel, TData, TModelReturn, TDataReturn>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
            CancellationToken cancellationToken = default)
            => await query.QueryAsync<TModel, TData, TModelReturn, TDataReturn, TModelReturn>(mapper, queryFunc, includeProperties, cancellationToken);

        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TModelReturn"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="queryFunc"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public static async Task<TModelReturn> QueryAsync<TModel, TData, TModelReturn>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
            CancellationToken cancellationToken = default)
            => await query.QueryAsync<TModel, TData, TModelReturn, TModelReturn, TModelReturn>(mapper, queryFunc, includeProperties, cancellationToken);
    }
}
