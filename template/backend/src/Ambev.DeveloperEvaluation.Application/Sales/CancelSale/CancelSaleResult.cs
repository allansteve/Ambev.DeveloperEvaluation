namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Response model for CancelSale operation
/// </summary>
public class CancelSaleResult
{
    /// <summary>
    /// Gets or sets the sale ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sale number
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the cancellation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the cancellation message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}