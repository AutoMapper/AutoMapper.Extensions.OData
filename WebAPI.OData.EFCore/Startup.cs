using AutoMapper;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace WebAPI.OData.EFCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //Rebuild
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MyDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddOData();

            services.AddSingleton<AutoMapper.IConfigurationProvider>
            (
                new MapperConfiguration(cfg =>
                {
                    cfg.AddMaps(typeof(Startup).Assembly);
                    cfg.AllowNullCollections = true;
                })
            )
            .AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService));

            services.AddMvc(options =>
            {
                // https://blogs.msdn.microsoft.com/webdev/2018/08/27/asp-net-core-2-2-0-preview1-endpoint-routing/
                // Because conflicts with ODataRouting as of this version
                // could improve performance though
                options.EnableEndpointRouting = false;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseODataBatching();

            app.UseMvc(r => {


                var builder = new ODataConventionModelBuilder()
                {
                    //Namespace = "OData",
                };
                var eb = builder.EntitySet<OpsTenant>(nameof(OpsTenant));
                builder.EntitySet<CoreBuilding>(nameof(CoreBuilding));
                builder.EntitySet<OpsBuilder>(nameof(OpsBuilder));
                builder.EntitySet<OpsCity>(nameof(OpsCity));
                var model = builder.GetEdmModel();
                r.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
                r.MapODataServiceRoute("odata", "", model, new DefaultODataBatchHandler());

            });

            //app.UseRouting();

            //app.UseEndpoints(routeBuilder =>
            //{
            //    routeBuilder.Expand().Select().Filter();

            //    routeBuilder.MapODataRoute(
            //        "OdataSimple",
            //        "OdataSimple",
            //        GetSimpleEdmModel("OdataSimple"));

            //    routeBuilder.MapODataRoute(
            //        "OdataAutomapper",
            //        "OdataAutomapper",
            //        GetMappedEdmModel("OdataAutomapper"));

            //});
        }
    }
}
