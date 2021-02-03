using System;
using AutoMapper.OData.Queryable.Tests.Data;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.OData.Queryable.Tests
{
    public abstract class TestsBase
    {
        protected readonly IServiceProvider serviceProvider;

        protected TestsBase()
        {
            serviceProvider = BuildServiceProvider();
        }

        private IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOData();

            RegisterDataContext(services);

            services
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

            return services.BuildServiceProvider();
        }

        protected virtual void RegisterDataContext(IServiceCollection services)
        {
            services.AddSingleton<IDataContext>(_ =>
            {
                var context = new InMemoryObjectContext();
                DatabaseInitializer.SeedDatabase(context);
                return context;
            });
        }
    }
}