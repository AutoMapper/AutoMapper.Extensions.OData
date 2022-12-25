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
    [Route("CoreBuilding")]
    public class CoreBuildingAllowApplyController : ODataController
    {
        private readonly MyDbContext _repository;
        private readonly IMapper _mapper;

        public CoreBuildingAllowApplyController(MyDbContext repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("AllowApply")]
        [AllowApply]
        public async Task<IActionResult> AllowApply(ODataQueryOptions<CoreBuilding> options)
        {
            return Ok(await _repository.BuildingSet.GetQueryAsync(_mapper, options, new QuerySettings { ODataSettings = new ODataSettings { HandleNullPropagation = HandleNullPropagationOption.False } }));
        }
    }
}
