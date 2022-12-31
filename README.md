# AutoMapper.Extensions.OData
Creates LINQ expressions from ODataQueryOptions and executes the query.

## NuGet Package Downloads

[![NuGet](http://img.shields.io/nuget/v/AutoMapper.AspNetCore.OData.EFCore?label=AutoMapper.AspNetCore.OData.EFCore)](https://www.nuget.org/packages/AutoMapper.AspNetCore.OData.EFCore/)  
[![NuGet](http://img.shields.io/nuget/v/AutoMapper.AspNetCore.OData.EF6?label=AutoMapper.AspNetCore.OData.EF6)](https://www.nuget.org/packages/AutoMapper.AspNetCore.OData.EF6/)  
[![NuGet](http://img.shields.io/nuget/v/AutoMapper.AspNet.OData.EF6?label=AutoMapper.AspNet.OData.EF6)](https://www.nuget.org/packages/AutoMapper.AspNet.OData.EF6/)  

## Usage
To use:
* Configure object maps with explicit expansion.
* Call the GetAsync or GetQueryAsync extension method from your OData controller.  IMapper is an AutoMapper interface.

```c#
    public class CoreBuildingMappings : AutoMapper.Profile
    {
        public CoreBuildingMappings()
        {
            CreateMap<TBuilding, CoreBuilding>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.LongName))
                .ForMember(d => d.Tenant, o => o.MapFrom(s => s.Mandator))
                .ForAllMembers(o => o.ExplicitExpansion());

            CreateMap<TBuilder, OpsBuilder>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<TCity, OpsCity>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
```

```c#
public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default);
public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options, HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default);
```

```c#
    public class CoreBuildingController : ODataController
    {

	private readonly IMapper _mapper;
        public CoreBuildingController(MyDbContext context, IMapper mapper)
        {
            Context = context;
            _mapper = mapper;
        }

        MyDbContext Context { get; set; }

        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<CoreBuilding> options)
        {
            return Ok(await Context.BuildingSet.GetQueryAsync(_mapper, options));
        }
    }
```

<br><br>
### Using the `EnableQueryAttribute`
Using `EnableQuery` with `AutoMapper.Extensions.OData` will result in some operations being applied more than once e.g. in the [tests](https://github.com/AutoMapper/AutoMapper.Extensions.OData/blob/5b4a9c8bef4c408268603e4c2186ca65b930559c/AutoMapper.OData.EFCore.Tests/AllTests.cs#L342),
if `TMandator` has a total of two records then **without** `EnableQuery` applied to the controller action, the OData query `http://localhost:16324/opstenant?$skip=1&$top=1&$orderby=Name` will return one record as expected. However **with** `EnableQuery` applied
no records will be returned because the skip operation has been applied twice.

While `EnableQuery` will result in some operations being applied more than once, one can create a custom `EnableQueryAttribute` to ignore the `QueryOptions` that are applied by `AutoMapper.Extensions.OData`.

```c#
   public class MyCustomEnableQueryAttribute : EnableQueryAttribute
    {
        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            var ignoreQueryOptions =
                AllowedQueryOptions.Skip |
                AllowedQueryOptions.Top |
                AllowedQueryOptions.Filter |
                AllowedQueryOptions.Expand |
                AllowedQueryOptions.Select |
                AllowedQueryOptions.OrderBy |
                AllowedQueryOptions.Count;

            return queryOptions.ApplyTo(queryable, ignoreQueryOptions);
        }
    }
```

### Aggregation
With a custom `EnableQueryAttribute` there is limited support for grouping and aggregation. Grouping and aggregation on navigation properties is not supported. Example can be found in the [`AggregationTests`](Web.Tests/AggregationTests.cs)

```c#
    public class AllowApplyAttribute : EnableQueryAttribute
    {
        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            var ignoreQueryOptions =
                AllowedQueryOptions.Skip |
                AllowedQueryOptions.Top |
                AllowedQueryOptions.Filter |
                AllowedQueryOptions.Expand |
                AllowedQueryOptions.Select |
                AllowedQueryOptions.OrderBy |
                AllowedQueryOptions.Count;

            return queryOptions.ApplyTo(queryable, ignoreQueryOptions);
        }

        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext is null)
            {
                throw new ArgumentNullException(nameof(actionExecutedContext));
            }

            if (actionExecutedContext.HttpContext.Response != null &&
                IsSuccessStatusCode(actionExecutedContext.HttpContext.Response.StatusCode) &&
                actionExecutedContext.Result is ObjectResult content &&
                content.Value != null &&
                content.DeclaredType == null)
            {
                // To help the `ODataOutputFormatter` to determine the correct output class
                // the `content.DeclaredType` needs to be set before appling the `$apply`-option
                // so that a valid `OData`-result is produced
                // https://github.com/OData/AspNetCoreOData/blob/4de92f52a346606a447ec4df96c5f3cd05642f50/src/Microsoft.AspNetCore.OData/Formatter/ODataOutputFormatter.cs#L127-L131
                content.DeclaredType = content.Value.GetType();
            }

            base.OnActionExecuted(actionExecutedContext);
        }
```

<br><br>
### OData query examples:

``` 
	http://localhost:<port>/opstenant?$top=5&$expand=Buildings&$filter=Name eq 'One'&$orderby=Name desc
	http://localhost:<port>/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc
	http://localhost:<port>/opstenant?$filter=Name eq 'One'
	http://localhost:<port>/opstenant?$top=5&$expand=Buildings&$orderby=Name desc
	http://localhost:<port>/opstenant?$orderby=Name desc
	http://localhost:<port>/opstenant?$orderby=Name desc&$count=true
	http://localhost:<port>/opstenant?$top=5&$filter=Name eq 'One'&$orderby=Name desc&$count=true
	http://localhost:<port>/opstenant?$top=5&$select=Name, Identity
	http://localhost:<port>/opstenant?$top=5&$expand=Buildings&$filter=Name ne 'One'&$orderby=Name desc
	http://localhost:<port>/opstenant?$top=5&$expand=Buildings($expand=Builder($expand=City))&$filter=Name ne 'One'&$orderby=Name desc
	http://localhost:<port>/opstenant?$top=5&$select=Buildings,Name&$expand=Buildings($select=Name,Builder;$expand=Builder($select=Name,City;$expand=City))&$filter=Name ne 'One'&$orderby=Name desc

	http://localhost:<port>/corebuilding?$top=5&$expand=Builder,Tenant&$filter=name ne 'One L1'&$orderby=Name desc
	http://localhost:<port>/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=name ne 'One L2'&$orderby=Name desc
```
