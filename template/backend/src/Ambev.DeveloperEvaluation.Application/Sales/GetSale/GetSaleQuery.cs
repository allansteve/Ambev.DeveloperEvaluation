using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Query for retrieving a sale by ID
/// </summary>
public class GetSaleQuery : IRequest<GetSaleResult?>
{
    /// <summary>
    /// Gets or sets the sale ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Initializes a new instance of GetSaleQuery
    /// </summary>
    /// <param name="id">The sale ID</param>
    public GetSaleQuery(Guid id)
    {
        Id = id;
    }
}