namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is modified
/// </summary>
public class SaleModifiedEvent
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
    /// Gets the new total sale amount
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// Gets the date when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new instance of SaleModifiedEvent
    /// </summary>
    public SaleModifiedEvent(Guid saleId, string saleNumber, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}