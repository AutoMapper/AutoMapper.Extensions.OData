using AutoMapper;
using AutoMapper.AspNet.OData;
using DAL.EF6.Aggregation;
using Domain.OData.Aggregation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Threading.Tasks;
using WebAPI.OData.EF6.Attributes;

namespace WebAPI.OData.EF6.Controllers
{
    public class SalesController : ODataController
    {
        private readonly IMapper _mapper;
        private readonly AggregationDbContext _context;

        public SalesController(AggregationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowApply]
        public async Task<IActionResult> Get(ODataQueryOptions<Sales> options)
        {
            return Ok(await _context.Sales.GetQueryAsync(_mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.Default } }));
        }
    }
}
