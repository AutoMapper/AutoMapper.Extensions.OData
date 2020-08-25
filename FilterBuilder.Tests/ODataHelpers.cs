using FilterBuilder.Tests.Data;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterBuilder.Tests
{
    public static class ODataHelpers
    {
        private static readonly IDictionary<Type, IEdmModel> cachedModels = new Dictionary<Type, IEdmModel>();
        private static IEdmModel GetModel<T>(IServiceProvider serviceProvider) where T : class
        {
            var modelType = typeof(T);

            if (cachedModels.TryGetValue(modelType, out IEdmModel cachedModel))
                return cachedModel;

            return GetModel(new ODataConventionModelBuilder(serviceProvider));

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

        public static FilterClause GetFilterClause<T>(string filter, IServiceProvider serviceProvider) where T : class
            => GetFilterClause<T>
            (
                new Dictionary<string, string> { ["$filter"] = filter },
                serviceProvider
            );

        public static FilterClause GetFilterClause<T>(IDictionary<string, string> queryOptions, IServiceProvider serviceProvider, bool useFilterOption = false) where T : class
        {
            IEdmModel model = GetModel<T>(serviceProvider);
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
            Microsoft.AspNet.OData.Routing.ODataPath path = new Microsoft.AspNet.OData.Routing.ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes), path);

            return new FilterQueryOption(filter, context, parser).FilterClause;
        }

        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString, IServiceProvider serviceProvider, IRouteBuilder routeBuilder) where T : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(serviceProvider);

            builder.EntitySet<T>(typeof(T).Name);
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            Microsoft.AspNet.OData.Routing.ODataPath path = new Microsoft.AspNet.OData.Routing.ODataPath(new EntitySetSegment(entitySet));

            routeBuilder.EnableDependencyInjection();

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
