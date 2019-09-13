using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.AspNet.OData.Query;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        /// <returns></returns>
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, IDictionary<string, object> parameters, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
            => Task.Run(async () => await query.GetAsync(mapper, options, parameters, handleNullPropagation)).Result;

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
            => Task.Run(async () => await query.GetAsync(mapper, options, null, handleNullPropagation)).Result;

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, IDictionary<string, object> parameters, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
        {
            Expression<Func<TModel, object>>[] includeExpressions = options.SelectExpand.GetIncludes().BuildIncludes<TModel>().ToArray();
            Expression<Func<TModel, bool>> filter = options.Filter.ToFilterExpression<TModel>(handleNullPropagation);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = options.GetQueryableExpression();

            ICollection<TModel> collection = await query.GetAsync(mapper, filter, queryableExpression, includeExpressions, parameters);

            return collection;
        }

        /// <summary>
        /// GetQueryAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
        {
            Expression<Func<TModel, object>>[] includeExpressions = options.SelectExpand.GetIncludes().BuildIncludes<TModel>().ToArray();
            Expression<Func<TModel, bool>> filter = options.Filter.ToFilterExpression<TModel>(handleNullPropagation);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = options.GetQueryableExpression();

            return await query.GetQueryAsync(mapper, filter, queryableExpression, includeExpressions);
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
            => Task.Run(async () => await query.GetAsync(mapper, filter, queryFunc, includeProperties)).Result;

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
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null,
            IDictionary<string, object> parameters = null)
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
            ICollection<TData> result = mappedQueryFunc != null ? await mappedQueryFunc(query).ToListAsync() : await query.ToListAsync();

            //Map and return the data            
            if (parameters == null)
                return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(result).ToList();

            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(result, o =>
            {
                foreach (var key in parameters.Keys)
                    o.Items[key] = parameters[key];
            }).ToList();
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
        /// <returns></returns>
        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();
            ICollection<Expression<Func<TData, object>>> includes = mapper.MapIncludesList<Expression<Func<TData, object>>>(includeProperties);

            if (filter != null)
                query = query.Where(f);

            if (includes != null)
                query = includes.Aggregate(query, (q, next) => q.Include(next));

            return await Task.Run
            (
                () => mappedQueryFunc != null
                    ? mapper.ProjectTo<TModel>(mappedQueryFunc(query))
                    : mapper.ProjectTo<TModel>(query)
            );
        }
    }
}
