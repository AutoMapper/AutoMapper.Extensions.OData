using AutoMapper;
using AutoMapper.AspNet.OData;
using DAL.EF6;
using Domain.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Threading.Tasks;
using WebAPI.OData.EF6.Attributes;

namespace WebAPI.OData.EF6.Controllers
{
    [Route("OpsTenant")]
    public class OpsTenantAllowApplyController : ODataController
    {
        private readonly IMapper _mapper;
        private readonly MyDbContext _context;

        public OpsTenantAllowApplyController(MyDbContext repository, IMapper mapper)
        {
            _context = repository;
            _mapper = mapper;
        }

        [HttpGet("AllowApply")]
        [AllowApply]
        public async Task<IActionResult> AllowApply(ODataQueryOptions<OpsTenant> options)
        {
            return Ok(await _context.MandatorSet.GetQueryAsync(_mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.Default } }));
        }
    }
}
