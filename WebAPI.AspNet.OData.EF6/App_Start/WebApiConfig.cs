using AutoMapper;
using Domain.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Unity;

namespace WebAPI.AspNet.OData.EF6
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            DAL.MyDbContext.DSN = @"data source=.\SQL2016;initial catalog=YourDB;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";//"Data Source=adeaeawe;Initial Catalog=aeaweawe;User ID=aeaweads;Password=aweasdawae; MultipleActiveResultSets=True";

            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<OpsTenant>(nameof(OpsTenant));
            builder.EntitySet<CoreBuilding>(nameof(CoreBuilding));
            builder.EntitySet<OpsBuilder>(nameof(OpsBuilder));
            builder.EntitySet<OpsCity>(nameof(OpsCity));
            config.MapODataServiceRoute("odata", "", builder.GetEdmModel());

            var mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddMaps(typeof(WebApiConfig));
            });

            var mapper = mapperConfig.CreateMapper();

            var container = new UnityContainer();
            container.RegisterInstance<IMapper>(mapperConfig.CreateMapper());
            config.DependencyResolver = new UnityResolver(container);
        }
    }
}
