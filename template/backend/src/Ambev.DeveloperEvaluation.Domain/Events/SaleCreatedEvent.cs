namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a new sale is created
/// </summary>
public class SaleCreatedEvent
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
    /// Gets the customer information
    /// </summary>
    public string Customer { get; }

    /// <summary>
    /// Gets the branch information
    /// </summary>
    public string Branch { get; }

    /// <summary>
    /// Gets the total sale amount
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// Gets the date when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new instance of SaleCreatedEvent
    /// </summary>
    public SaleCreatedEvent(Guid saleId, string saleNumber, string customer, string branch, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        Customer = customer;
        Branch = branch;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}