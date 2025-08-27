using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an item within a sale, containing product information and quantities
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the sale ID this item belongs to
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the product ID (external identity)
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name (denormalized for external identity pattern)
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product being sold
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage applied to this item (0-100)
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Gets the total amount for this item after discount
    /// </summary>
    public decimal TotalAmount => Quantity * UnitPrice * (1 - DiscountPercentage / 100);

    /// <summary>
    /// Gets or sets whether this item is cancelled
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Navigation property to the parent sale
    /// </summary>
    public Sale Sale { get; set; } = null!;

    /// <summary>
    /// Validates the sale item according to business rules
    /// </summary>
    public bool IsValid(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (Quantity <= 0)
        {
            errorMessage = "Quantity must be greater than 0";
            return false;
        }

        if (Quantity > 20)
        {
            errorMessage = "Cannot sell more than 20 identical items";
            return false;
        }

        if (UnitPrice <= 0)
        {
            errorMessage = "Unit price must be greater than 0";
            return false;
        }

        if (Quantity < 4 && DiscountPercentage > 0)
        {
            errorMessage = "Purchases below 4 items cannot have a discount";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates the appropriate discount percentage based on quantity
    /// </summary>
    public decimal CalculateDiscount()
    {
        if (Quantity < 4)
            return 0;
        
        if (Quantity >= 10 && Quantity <= 20)
            return 20;
        
        if (Quantity >= 4)
            return 10;

        return 0;
    }

    /// <summary>
    /// Applies the appropriate discount based on business rules
    /// </summary>
    public void ApplyDiscount()
    {
        DiscountPercentage = CalculateDiscount();
    }

    /// <summary>
    /// Cancels this sale item
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }
}