using AutoMapper;
using DAL.EFCore.Aggregation;
using Domain.OData.Aggregation;

namespace WebAPI.OData.EFCore.Mappings
{
    public class AggregationMappings : Profile
    {
        public AggregationMappings()
        {
            CreateMap<TblCategory, Category>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.FldId))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.FldName))
                .ForAllMembers(options => options.ExplicitExpansion());

            CreateMap<TblCurrency, Currency>()
                .ForMember(destination => destination.Code, options => options.MapFrom(source => source.FldCode))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.FldName))
                .ForAllMembers(options => options.ExplicitExpansion());

            CreateMap<TblCustomer, Customer>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.FldId))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.FldName))
                .ForMember(destination => destination.Country, options => options.MapFrom(source => source.FldCountry))
                .ForAllMembers(options => options.ExplicitExpansion());

            CreateMap<TblProduct, Product>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.FldId))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.FldName))
                .ForMember(destination => destination.Color, options => options.MapFrom(source => source.FldColor))
                .ForMember(destination => destination.TaxRate, options => options.MapFrom(source => source.FldTaxRate))
                .ForAllMembers(options => options.ExplicitExpansion());

            CreateMap<TblSales, Sales>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.FldId))
                .ForMember(destination => destination.Amount, options => options.MapFrom(source => source.FldAmount))
                .ForAllMembers(options => options.ExplicitExpansion());

            CreateMap<TblSalesOrganization, SalesOrganization>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.FldId))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.FldName))
                .ForAllMembers(options => options.ExplicitExpansion());

            CreateMap<TblTime, Time>();
        }
    }
}
