using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// AutoMapper profile for UpdateSale API operations
/// </summary>
public class UpdateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mapping configuration for UpdateSale API
    /// </summary>
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<UpdateSaleItemRequest, UpdateSaleItemCommand>();

        CreateMap<UpdateSaleResult, UpdateSaleResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<UpdateSaleItemResult, UpdateSaleItemResponse>();
    }
}