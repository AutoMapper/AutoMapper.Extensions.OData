using AutoMapper.AspNet.OData.Visitors;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.AspNet.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AutoMapper.AspNet.OData
{
    public static class QueryableExtensions
    {
        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
            => Task.Run(async () => await query.GetAsync(mapper, options, handleNullPropagation)).Result;

        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
            => query.Get(mapper, options, querySettings == null ? HandleNullPropagationOption.Default : querySettings.HandleNullPropagation);

        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
        {
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeExpressions = options.SelectExpand.GetIncludes().BuildIncludesExpressionCollection<TModel>()?.ToList();
            Expression<Func<TModel, bool>> filter = options.Filter.ToFilterExpression<TModel>(handleNullPropagation);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = options.GetQueryableExpression();
            Expression<Func<IQueryable<TModel>, long>> countExpression = LinqExtensions.GetCountExpression<TModel>(filter);

            options.AddExpandOptionsResult();
            if (options.Count?.Value == true)
                options.AddCountOptionsResult<TModel, TData>(await query.QueryAsync(mapper, countExpression));

            return await query.GetAsync(mapper, filter, queryableExpression, includeExpressions);
        }

        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
            => await query.GetAsync(mapper, options, querySettings == null ? HandleNullPropagationOption.Default : querySettings.HandleNullPropagation);

        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
        {
            var expansions = options.SelectExpand.GetExpansions(typeof(TModel));
            List<Expression<Func<TModel, object>>> includeExpressions = expansions.Select(list => new List<Expansion>(list)).BuildIncludes<TModel>
            (
                options.SelectExpand.GetSelects()
            )
            .ToList();

            Expression<Func<TModel, bool>> filter = options.Filter.ToFilterExpression<TModel>(handleNullPropagation);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = options.GetQueryableExpression();
            Expression<Func<IQueryable<TModel>, long>> countExpression = LinqExtensions.GetCountExpression<TModel>(filter);

            options.AddExpandOptionsResult();
            if (options.Count?.Value == true)
                options.AddCountOptionsResult<TModel, TData>(await query.QueryAsync(mapper, countExpression));

            IQueryable<TModel> queryable = await query.GetQueryAsync(mapper, filter, queryableExpression, includeExpressions);

            return queryable.UpdateQueryableExpression(expansions);
        }

        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, QuerySettings querySettings = null)
            where TModel : class
            => await query.GetQueryAsync(mapper, options, querySettings == null ? HandleNullPropagationOption.Default : querySettings.HandleNullPropagation);

        public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            => Task.Run(async () => await query.GetAsync(mapper, filter, queryFunc, includeProperties)).Result;


        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            if (filter != null)
                query = query.Where(f);

            if (includes != null)
                query = includes.Select(i => i.Compile()).Aggregate(query, (q, next) => q = next(q));

            //Call the store
            ICollection<TData> result = mappedQueryFunc != null ? await mappedQueryFunc(query).ToListAsync() : await query.ToListAsync();

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(result).ToList();
        }

        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            IEnumerable<Expression<Func<TModel, object>>> includeProperties = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            if (filter != null)
                query = query.Where(f);

            return await Task.Run
            (
                () => mappedQueryFunc != null
                    ? mapper.ProjectTo(mappedQueryFunc(query), null, GetIncludes())
                    : mapper.ProjectTo(query, null, GetIncludes())
            );

            Expression<Func<TModel, object>>[] GetIncludes() => includeProperties?.ToArray() ?? new Expression<Func<TModel, object>>[] { };
        }

        public static async Task<TReturn> QueryAsync<TModel, TData, TModelReturn, TDataReturn, TReturn>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            Func<IQueryable<TData>, TDataReturn> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataReturn>>>(queryFunc).Compile();
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            if (includes != null)
                query = includes.Select(i => i.Compile()).Aggregate(query, (q, next) => q = next(q));

            TDataReturn result = await Task.Run(() => mappedQueryFunc(query));

            return typeof(TReturn) == typeof(TDataReturn) ? (TReturn)(object)result : mapper.Map<TDataReturn, TReturn>(result);
        }

        public static async Task<TModelReturn> QueryAsync<TModel, TData, TModelReturn, TDataReturn>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            => await query.QueryAsync<TModel, TData, TModelReturn, TDataReturn, TModelReturn>(mapper, queryFunc, includeProperties);

        public static async Task<TModelReturn> QueryAsync<TModel, TData, TModelReturn>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelReturn>> queryFunc,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            => await query.QueryAsync<TModel, TData, TModelReturn, TModelReturn, TModelReturn>(mapper, queryFunc, includeProperties);
    }
}
