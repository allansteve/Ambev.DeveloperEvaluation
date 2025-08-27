using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// AutoMapper profile for UpdateSale operations
/// </summary>
public class UpdateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mapping configuration for UpdateSale
    /// </summary>
    public UpdateSaleProfile()
    {
        CreateMap<Sale, UpdateSaleResult>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<SaleItem, UpdateSaleItemResult>();
    }
}