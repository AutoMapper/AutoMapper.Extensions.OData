using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;

namespace AutoMapper.OData.Queryable.Tests
{
    public static class ODataHelpers
    {
        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString, IServiceProvider serviceProvider, IRouteBuilder routeBuilder) where T : class
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(serviceProvider);

            builder.EntitySet<T>(typeof(T).Name);
            builder.EnableLowerCamelCase();
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(typeof(T).Name);
            ODataPath path = new ODataPath(new Microsoft.OData.UriParser.EntitySetSegment(entitySet));

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