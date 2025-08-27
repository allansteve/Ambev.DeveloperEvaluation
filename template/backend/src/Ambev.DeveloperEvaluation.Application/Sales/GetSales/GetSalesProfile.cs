using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// AutoMapper profile for GetSales feature mapping
/// </summary>
public class GetSalesProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetSales feature
    /// </summary>
    public GetSalesProfile()
    {
        CreateMap<Sale, SaleListItem>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count(i => !i.IsCancelled)));
    }
}