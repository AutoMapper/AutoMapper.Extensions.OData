using ExpressionBuilder.Tests.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressionBuilder.Tests
{
    public static class ODataHelpers
    {
        private static readonly IDictionary<Type, IEdmModel> cachedModels = new Dictionary<Type, IEdmModel>();
        private static IEdmModel GetModel<T>() where T : class
        {
            var modelType = typeof(T);

            if (cachedModels.TryGetValue(modelType, out IEdmModel cachedModel))
                return cachedModel;

            return GetModel(new ODataConventionModelBuilder());

            IEdmModel GetModel(ODataConventionModelBuilder builder)
            {
                builder.EntitySet<T>(modelType.Name);
                if (modelType == typeof(Product))
                {
                    builder.EntityType<DerivedProduct>().DerivesFrom<Product>();
                    builder.EntityType<DerivedCategory>().DerivesFrom<Category>();
                }

                cachedModels.Add(modelType, builder.GetEdmModel());

                return cachedModels[modelType];
            }
        }

        public static SelectExpandClause GetSelectExpandClause<T>(IDictionary<string, string> queryOptions) where T : class
        {
            IEdmModel model = GetModel<T>();
            IEdmEntityType productType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == typeof(T).Name);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);

            ODataQueryOptionParser parser = new
            (
                model,
                productType,
                entitySet,
                queryOptions
            );

            return parser.ParseSelectAndExpand();
        }

        public static FilterClause GetFilterClause<T>(string filter, IServiceProvider serviceProvider) where T : class
            => GetFilterClause<T>
            (
                new Dictionary<string, string> { ["$filter"] = filter },
                serviceProvider
            );

        public static FilterClause GetFilterClause<T>(IDictionary<string, string> queryOptions, IServiceProvider serviceProvider, bool useFilterOption = false) where T : class
        {
            IEdmModel model = GetModel<T>();
            IEdmEntityType productType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == typeof(T).Name);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);

            ODataQueryOptionParser parser = new ODataQueryOptionParser
            (
                model,
                productType,
                entitySet,
                queryOptions
            );

            if (useFilterOption)
            {
                return GetFilterClauseFromFilterOption
                (
                    model,
                    entitySet,
                    parser,
                    queryOptions["$filter"]
                );
            }

            return parser.ParseFilter();
        }

        public static FilterClause GetFilterClauseFromFilterOption(IEdmModel model, IEdmEntitySet entitySet, ODataQueryOptionParser parser, string filter)
        {
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext context = new(model, typeof(DataTypes), path);

            return new FilterQueryOption(filter, context, parser).FilterClause;
        }

        public static ODataQueryContext GetODataQueryContext<T>() where T : class
        {
            IEdmModel model = GetModel<T>();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            return new ODataQueryContext(model, typeof(T), new ODataPath(new EntitySetSegment(entitySet)));
        }

        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString, IServiceProvider serviceProvider, IRouteBuilder routeBuilder) where T : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<T>(typeof(T).Name);
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));


            return new ODataQueryOptions<T>
            (
                new ODataQueryContext(model, typeof(T), path),
                BuildRequest
                (
                    new DefaultHttpContext()
                    {
                        RequestServices = serviceProvider
                    }.Request,
                    new Uri(BASEADDRESS + queryString)
                )
            );

            static HttpRequest BuildRequest(HttpRequest request, Uri uri)
            {
                request.Method = "GET";
                request.Host = new HostString(uri.Host, uri.Port);
                request.Path = uri.LocalPath;
                request.QueryString = new QueryString(uri.Query);

                return request;
            }
        }

        static readonly string BASEADDRESS = "http://localhost:16324";
    }
}
