using AutoMapper;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using WebAPI.OData.EFCore.Binders;
using WebAPI.OData.EFCore.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers()
    .AddOData(opt => opt.EnableQueryFeatures()
        .AddRouteComponents("", GetEdmModel(), services => services.AddSingleton<ISearchBinder, OpsTenantSearchBinder>()));
builder.Services.AddSingleton<AutoMapper.IConfigurationProvider>
(
    new MapperConfiguration(cfg =>
    {
        cfg.AddMaps(typeof(ValuesController).Assembly);
        cfg.AllowNullCollections = true;
    }, new NullLoggerFactory())
)
.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService));

var app = builder.Build();

if (builder.Environment.EnvironmentName == "Development")
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EnableLowerCamelCase();
    builder.EntitySet<OpsTenant>(nameof(OpsTenant));
    builder.EntitySet<CoreBuilding>(nameof(CoreBuilding));
    builder.EntitySet<OpsBuilder>(nameof(OpsBuilder));
    builder.EntitySet<OpsCity>(nameof(OpsCity));

    return builder.GetEdmModel();
}