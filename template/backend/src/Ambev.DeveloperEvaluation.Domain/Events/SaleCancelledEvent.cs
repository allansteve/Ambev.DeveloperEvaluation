namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is cancelled
/// </summary>
public class SaleCancelledEvent
{
    /// <summary>
    /// Gets the sale ID
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Gets the sale number
    /// </summary>
    public string SaleNumber { get; }

    /// <summary>
    /// Gets the date when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new instance of SaleCancelledEvent
    /// </summary>
    public SaleCancelledEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}