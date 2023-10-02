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
            int? currentUserFavoriteCategory = null;
            string currentUserState = null;

            CreateMap<Address, AddressModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<Category, CategoryModel>()
                .ForMember(
                    categoryModel => categoryModel.IsFavorite,
                    o => o.MapFrom(
                        category => currentUserFavoriteCategory.HasValue
                                    && category.CategoryID == currentUserFavoriteCategory.Value))
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
                .ForMember(
                    productModel => productModel.IsShippableToUser,
                    o => o.MapFrom(
                        product => !string.IsNullOrEmpty(currentUserState)
                                   && product.SupplierAddress.State == currentUserState))
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<CompositeKey, CompositeKeyModel>();
        }
    }
}
