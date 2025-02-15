using AutoMapper.OData.EFCore.Tests.Data;
using AutoMapper.OData.EFCore.Tests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoMapper.OData.EFCore.Tests.Mappings
{
    public class ObjectMappings : Profile
    {
        public ObjectMappings()
        {
            CreateMap<Address, AddressModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Category, CategoryModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<SuperCategory, SuperCategoryModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DataTypes, DataTypesModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DerivedCategory, DerivedCategoryModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DerivedProduct, DerivedProductModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DynamicProduct, DynamicProductModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Product, ProductModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<CompositeKey, CompositeKeyModel>();

            CreateMap<Organization, OrganizationDto>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<OrganizationName, OrganizationNameDto>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<OrganizationIdentity, OrganizationIdentityDto>()
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
}
