﻿using AutoMapper;
using AutoMapper.AspNet.OData;
using DAL.EF6;
using Domain.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.OData.EF6.Controllers
{
    public class OpsTenantController : ODataController
    {
        private readonly IMapper _mapper;
        private readonly MyDbContext _context;

        public OpsTenantController(MyDbContext repository, IMapper mapper)
        {
            _context = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<OpsTenant> options)
        {
            return Ok(await _context.MandatorSet.GetQueryAsync(_mapper, options, new QuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }));
        }
    }

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
            return Ok(await _context.BuildingSet.GetQueryAsync(_mapper, options, new QuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }));
        }
    }
}
