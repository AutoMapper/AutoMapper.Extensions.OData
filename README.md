# AutoMapper.Extensions.OData
Creates LINQ expressions from ODataQueryOptions and executes the query.

To use, call the Get or GetAsync extension method from your OData controller.  IMapper is an AutoMapper interface.

```c#
public static ICollection<TModel> Get<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options);
public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper, ODataQueryOptions<TModel> options)
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
        [EnableQuery(MaxExpansionDepth = 5)]
        public async Task<IActionResult> Get(ODataQueryOptions<CoreBuilding> options)
        {
            return Ok(await Context.BuildingSet.GetAsync(_mapper, options));
        }
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

	http://localhost:<port>/corebuilding?$top=5&$expand=Builder,Tenant&$filter=name ne 'One L1'&$orderby=Name desc
	http://localhost:<port>/corebuilding?$top=5&$expand=Builder($expand=City),Tenant&$filter=name ne 'One L2'&$orderby=Name desc
```
