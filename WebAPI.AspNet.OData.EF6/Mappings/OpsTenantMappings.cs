using AutoMapper;
using DAL;
using Domain.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.AspNet.OData.EF6.Mappings
{
    public class OpsTenantMappings : Profile
    {
        public OpsTenantMappings()
        {
            CreateMap<TMandator, OpsTenant>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
}