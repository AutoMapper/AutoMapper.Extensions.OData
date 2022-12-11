using AutoMapper;
using AutoMapper.AspNet.OData;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Threading.Tasks;
using WebAPI.OData.EFCore.Attributes;

namespace WebAPI.OData.EFCore.Controllers
{
    [Route("OpsTenant")]
    public class OpsTenantController : ODataController
    {
        private readonly IMapper _mapper;

        public OpsTenantController(MyDbContext repository, IMapper mapper)
        {
            Repository = repository;
            _mapper = mapper;
        }

        MyDbContext Repository { get; set; }

        [HttpGet("WithoutEnableQuery")]
        public async Task<IActionResult> WithoutEnableQuery(ODataQueryOptions<OpsTenant> options)
        {
            return Ok(await Repository.MandatorSet.GetQueryAsync(_mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False } }));
        }

        [HttpGet("WithEnableQuery")]
        [AutomapperEnableQuery(MaxExpansionDepth = 10)]
        public async Task<IActionResult> WithEnableQuery(ODataQueryOptions<OpsTenant> options)
        {
            return Ok(await Repository.MandatorSet.GetQueryAsync(_mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False } }));
        }
    }
}
