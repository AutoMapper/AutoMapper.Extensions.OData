using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DAL;
using Domain.OData;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebAPI.AspNetCore.OData.EF6
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            MyDbContext.DSN = @"data source=.\SQL2014;initial catalog=Issue3;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
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
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseODataBatching();

            app.UseMvc(r => {
                var builder = new ODataConventionModelBuilder()
                {
                };
                var eb = builder.EntitySet<OpsTenant>(nameof(OpsTenant));
                builder.EntitySet<CoreBuilding>(nameof(CoreBuilding));
                builder.EntitySet<OpsBuilder>(nameof(OpsBuilder));
                builder.EntitySet<OpsCity>(nameof(OpsCity));
                var model = builder.GetEdmModel();
                r.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
                r.MapODataServiceRoute("odata", "", model, new DefaultODataBatchHandler());

            });
        }
    }
}
