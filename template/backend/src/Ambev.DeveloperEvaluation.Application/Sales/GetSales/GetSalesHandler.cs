using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Handler for processing GetSalesQuery requests
/// </summary>
public class GetSalesHandler : IRequestHandler<GetSalesQuery, GetSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetSalesHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetSalesQuery request
    /// </summary>
    /// <param name="query">The GetSales query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The list of sales</returns>
    public async Task<GetSalesResult> Handle(GetSalesQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Sale> sales;

        // Apply filters based on query parameters
        if (!string.IsNullOrEmpty(query.Customer))
        {
            sales = await _saleRepository.GetByCustomerAsync(query.Customer, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(query.Branch))
        {
            sales = await _saleRepository.GetByBranchAsync(query.Branch, cancellationToken);
        }
        else if (query.StartDate.HasValue && query.EndDate.HasValue)
        {
            sales = await _saleRepository.GetByDateRangeAsync(query.StartDate.Value, query.EndDate.Value, cancellationToken);
        }
        else
        {
            var skip = (query.Page - 1) * query.Size;
            sales = await _saleRepository.GetAllAsync(skip, query.Size, cancellationToken);
        }

        var salesList = sales.ToList();
        var totalCount = salesList.Count;

        // Apply pagination if not already applied
        if (string.IsNullOrEmpty(query.Customer) && string.IsNullOrEmpty(query.Branch) && 
            !query.StartDate.HasValue && !query.EndDate.HasValue)
        {
            // Pagination was already applied in repository
        }
        else
        {
            // Apply pagination to filtered results
            var skip = (query.Page - 1) * query.Size;
            salesList = salesList.Skip(skip).Take(query.Size).ToList();
        }

        var result = new GetSalesResult
        {
            Sales = _mapper.Map<List<SaleListItem>>(salesList),
            CurrentPage = query.Page,
            PageSize = query.Size,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.Size)
        };

        return result;
    }
}