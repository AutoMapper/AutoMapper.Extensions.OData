using AutoMapper;
using AutoMapper.AspNet.OData;
using DAL.EF6;
using Domain.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Threading.Tasks;

namespace WebAPI.OData.EF6.Controllers
{
    public class CoreBuildingController : ODataController
    {
        private readonly IMapper _mapper;
        private readonly MyDbContext _context;

        public CoreBuildingController(MyDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<CoreBuilding> options)
        {
            return Ok(await _context.BuildingSet.GetQueryAsync(_mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.Default } }));
        }
    }
}
