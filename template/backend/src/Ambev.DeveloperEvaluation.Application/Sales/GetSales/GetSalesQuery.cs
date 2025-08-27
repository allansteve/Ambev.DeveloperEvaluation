using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Query for retrieving multiple sales with filtering options
/// </summary>
public class GetSalesQuery : IRequest<GetSalesResult>
{
    /// <summary>
    /// Gets or sets the page number (starts from 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int Size { get; set; } = 10;

    /// <summary>
    /// Gets or sets the customer filter
    /// </summary>
    public string? Customer { get; set; }

    /// <summary>
    /// Gets or sets the branch filter
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets the start date filter
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date filter
    /// </summary>
    public DateTime? EndDate { get; set; }
}