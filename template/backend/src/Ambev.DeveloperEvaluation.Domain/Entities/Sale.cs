using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale transaction with all business rules and domain logic
/// </summary>
public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = new();
    private readonly List<object> _domainEvents = new();

    /// <summary>
    /// Gets or sets the sale number (unique identifier for business purposes)
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the sale was made
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the customer information (external identity)
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch where the sale was made (external identity)
    /// </summary>
    public string Branch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sale status
    /// </summary>
    public SaleStatus Status { get; set; }

    /// <summary>
    /// Gets the sale items
    /// </summary>
    public IReadOnlyList<SaleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the total sale amount (calculated from items)
    /// </summary>
    public decimal TotalAmount => _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);

    /// <summary>
    /// Gets domain events that have occurred
    /// </summary>
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets or sets the date when the sale was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date when the sale was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of Sale
    /// </summary>
    public Sale()
    {
        Status = SaleStatus.Active;
        SaleDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an item to the sale with business rule validation
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="productName">The product name</param>
    /// <param name="quantity">The quantity</param>
    /// <param name="unitPrice">The unit price</param>
    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Cannot add items to a cancelled sale");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        
        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            if (newQuantity > 20)
                throw new InvalidOperationException("Cannot sell more than 20 identical items");
            
            existingItem.Quantity = newQuantity;
            existingItem.ApplyDiscount();
        }
        else
        {
            var item = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = Id,
                ProductId = productId,
                ProductName = productName,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Sale = this
            };

            if (!item.IsValid(out string errorMessage))
                throw new InvalidOperationException(errorMessage);

            item.ApplyDiscount();
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber, TotalAmount));
    }

    /// <summary>
    /// Removes an item from the sale
    /// </summary>
    /// <param name="itemId">The item ID to remove</param>
    public void RemoveItem(Guid itemId)
    {
        if (Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Cannot remove items from a cancelled sale");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber, TotalAmount));
    }

    /// <summary>
    /// Updates the quantity of an existing item
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="newQuantity">The new quantity</param>
    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        if (Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Cannot update items in a cancelled sale");

        var item = _items.FirstOrDefault(i => i.Id == itemId && !i.IsCancelled);
        if (item == null)
            throw new InvalidOperationException("Item not found or already cancelled");

        if (newQuantity <= 0)
        {
            CancelItem(itemId);
            return;
        }

        if (newQuantity > 20)
            throw new InvalidOperationException("Cannot sell more than 20 identical items");

        item.Quantity = newQuantity;
        item.ApplyDiscount();
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber, TotalAmount));
    }

    /// <summary>
    /// Cancels a specific item in the sale
    /// </summary>
    /// <param name="itemId">The item ID to cancel</param>
    public void CancelItem(Guid itemId)
    {
        if (Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel items from a cancelled sale");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found");

        if (item.IsCancelled)
            throw new InvalidOperationException("Item is already cancelled");

        item.Cancel();
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ItemCancelledEvent(Id, itemId, item.ProductName, item.Quantity));
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber, TotalAmount));
    }

    /// <summary>
    /// Cancels the entire sale
    /// </summary>
    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Sale is already cancelled");

        Status = SaleStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleCancelledEvent(Id, SaleNumber));
    }

    /// <summary>
    /// Validates the sale according to business rules
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SaleNumber))
            errors.Add("Sale number is required");

        if (string.IsNullOrWhiteSpace(Customer))
            errors.Add("Customer is required");

        if (string.IsNullOrWhiteSpace(Branch))
            errors.Add("Branch is required");

        if (!_items.Any(i => !i.IsCancelled))
            errors.Add("Sale must have at least one active item");

        foreach (var item in _items.Where(i => !i.IsCancelled))
        {
            if (!item.IsValid(out string itemError))
                errors.Add($"Item {item.ProductName}: {itemError}");
        }

        return !errors.Any();
    }

    /// <summary>
    /// Creates a sale and raises the SaleCreated event
    /// </summary>
    public static Sale Create(string saleNumber, string customer, string branch)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            Customer = customer,
            Branch = branch
        };

        sale.AddDomainEvent(new SaleCreatedEvent(sale.Id, saleNumber, customer, branch, 0));
        return sale;
    }

    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    private void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (typically called after publishing)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}