namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale item is cancelled
/// </summary>
public class ItemCancelledEvent
{
    /// <summary>
    /// Gets the sale ID
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Gets the sale item ID
    /// </summary>
    public Guid SaleItemId { get; }

    /// <summary>
    /// Gets the product name
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the cancelled quantity
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Gets the date when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new instance of ItemCancelledEvent
    /// </summary>
    public ItemCancelledEvent(Guid saleId, Guid saleItemId, string productName, int quantity)
    {
        SaleId = saleId;
        SaleItemId = saleItemId;
        ProductName = productName;
        Quantity = quantity;
        OccurredAt = DateTime.UtcNow;
    }
}