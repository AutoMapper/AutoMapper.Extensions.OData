using AutoMapper.AspNet.OData.Visitors;
using AutoMapper.Extensions.ExpressionMapping;
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
            => Task.Run(async () => await query.GetAsync(mapper, options, handleNullPropagation)).Result;

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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

        public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default)
            where TModel : class
        {
            var expansions = options.SelectExpand.GetExpansions(typeof(TModel));
            List<Expression<Func<TModel, object>>> includeExpressions = expansions.BuildIncludes<TModel>
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

            return queryable.UpdateQueryable(expansions);
        }

        private static IQueryable<TModel> UpdateQueryable<TModel>(this IQueryable<TModel> query, List<List<Expansion>> expansions)
        {
            List<List<Expansion>> filters = GetFilters();
            List<List<Expansion>> methods = GetQueryMethods();

            if (!filters.Any() && !methods.Any())
                return query;

            Expression expression = query.Expression;

            if (filters.Any())
                expression = UpdateProjectionFilterExpression(expression);

            if (methods.Any())
                expression = UpdateProjectionMethodExpression(expression);

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

            List<List<Expansion>> GetFilters()
                => expansions.Aggregate(new List<List<Expansion>>(), (listOfLists, nextList) =>
                {
                    var filterNextList = nextList.Aggregate(new List<Expansion>(), (list, next) =>
                    {
                        if (next.FilterOptions != null)
                        {
                            list = list.ConvertAll
                            (
                                exp => new Expansion
                                {
                                    MemberName = exp.MemberName,
                                    MemberType = exp.MemberType,
                                    ParentType = exp.ParentType,
                                }
                            );//new list removing filter

                            list.Add
                            (
                                new Expansion
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

            List<List<Expansion>> GetQueryMethods()
                => expansions.Aggregate(new List<List<Expansion>>(), (listOfLists, nextList) =>
                {
                    var filterNextList = nextList.Aggregate(new List<Expansion>(), (list, next) =>
                    {
                        if (next.QueryOptions != null)
                        {
                            list = list.ConvertAll
                            (
                                exp => new Expansion
                                {
                                    MemberName = exp.MemberName,
                                    MemberType = exp.MemberType,
                                    ParentType = exp.ParentType,
                                }
                            );//new list removing query options

                            list.Add
                            (
                                new Expansion
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
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
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
        /// <returns></returns>
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
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            Func<IQueryable<TData>, TDataReturn> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataReturn>>>(queryFunc).Compile();
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            if (includes != null)
                query = includes.Select(i => i.Compile()).Aggregate(query, (q, next) => q = next(q));

            TDataReturn result = await Task.Run(() => mappedQueryFunc(query));

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
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            => await query.QueryAsync<TModel, TData, TModelReturn, TDataReturn, TModelReturn>(mapper, queryFunc, includeProperties);

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
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
            => await query.QueryAsync<TModel, TData, TModelReturn, TModelReturn, TModelReturn>(mapper, queryFunc, includeProperties);
    }
}
