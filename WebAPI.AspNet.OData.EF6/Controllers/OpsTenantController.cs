using AutoMapper;
using AutoMapper.AspNet.OData;
using Domain.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebAPI.AspNet.OData.EF6.Controllers
{
    public class CoreBuildingController : ODataController
    {
        private readonly IMapper _mapper;

        public CoreBuildingController(IMapper mapper)
        {
            Repository = new DAL.MyDbContext();
            this._mapper = mapper;
        }

        DAL.MyDbContext Repository { get; set; }

        [HttpGet]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<CoreBuilding> options)
        {
            return Ok(await Repository.BuildingSet.GetQueryAsync(this._mapper, options, HandleNullPropagationOption.False));
        }
    }

    public class OpsTenantController : ODataController
    {
        private readonly IMapper _mapper;

        public OpsTenantController(IMapper mapper)
        {
            Repository = new DAL.MyDbContext();
            this._mapper = mapper;
        }

        DAL.MyDbContext Repository { get; set; }

        [HttpGet]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<OpsTenant> options)
        {
            return Ok(await Repository.MandatorSet.GetQueryAsync(this._mapper, options, HandleNullPropagationOption.False));
        }
    }
}
