using AutoMapper.OData.Queryable.Tests.Data;
using AutoMapper.OData.Queryable.Tests.Model;

namespace AutoMapper.OData.Queryable.Tests.Mappings
{
    public class ObjectMappings : Profile
    {
        public ObjectMappings()
        {
            CreateMap<Address, AddressModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Category, CategoryModel>()
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
        }
    }
}
