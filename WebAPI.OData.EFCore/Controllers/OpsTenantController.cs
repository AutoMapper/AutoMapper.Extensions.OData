using AutoMapper;
using AutoMapper.AspNet.OData;
using DAL.EFCore;
using Domain.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.OData.EFCore.Controllers
{
    public class OpsTenantController : ODataController
    {
        private readonly IMapper _mapper;

        public OpsTenantController(MyDbContext repository, IMapper mapper)
        {
            Repository = repository;
            _mapper = mapper;
        }

        MyDbContext Repository { get; set; }


        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<OpsTenant> options)
        {
            return Ok(await Repository.MandatorSet.GetQueryAsync(_mapper, options, HandleNullPropagationOption.False));
        }
    }

    public class CoreBuildingController : ODataController
    {
        private readonly IMapper _mapper;
        public CoreBuildingController(MyDbContext repository, IMapper mapper)
        {
            Repository = repository;
            _mapper = mapper;
        }

        MyDbContext Repository { get; set; }

        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<CoreBuilding> options)
        {
            return Ok(await Repository.BuildingSet.GetQueryAsync(_mapper, options, HandleNullPropagationOption.False));
        }
    }
}