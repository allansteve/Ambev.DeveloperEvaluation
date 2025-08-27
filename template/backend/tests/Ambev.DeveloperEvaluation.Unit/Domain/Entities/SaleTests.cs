using Xunit;
using FluentAssertions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Tests for Sale entity business rules and domain logic
/// </summary>
public class SaleTests
{
    [Fact]
    public void Create_ShouldCreateSaleWithActiveStatus()
    {
        // Arrange
        var saleNumber = "SALE-001";
        var customer = "Test Customer";
        var branch = "Test Branch";

        // Act
        var sale = Sale.Create(saleNumber, customer, branch);

        // Assert
        sale.Should().NotBeNull();
        sale.SaleNumber.Should().Be(saleNumber);
        sale.Customer.Should().Be(customer);
        sale.Branch.Should().Be(branch);
        sale.Status.Should().Be(SaleStatus.Active);
        sale.Items.Should().BeEmpty();
        sale.TotalAmount.Should().Be(0);
        sale.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_WithValidData_ShouldAddItemWithCorrectDiscount()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 5; // Should get 10% discount
        var unitPrice = 100m;

        // Act
        sale.AddItem(productId, productName, quantity, unitPrice);

        // Assert
        sale.Items.Should().HaveCount(1);
        var item = sale.Items.First();
        item.Quantity.Should().Be(quantity);
        item.UnitPrice.Should().Be(unitPrice);
        item.DiscountPercentage.Should().Be(10); // 4+ items = 10% discount
        item.TotalAmount.Should().Be(450); // 5 * 100 * 0.9
        sale.TotalAmount.Should().Be(450);
    }

    [Fact]
    public void AddItem_WithQuantityBetween10And20_ShouldApply20PercentDiscount()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 15; // Should get 20% discount
        var unitPrice = 100m;

        // Act
        sale.AddItem(productId, productName, quantity, unitPrice);

        // Assert
        var item = sale.Items.First();
        item.DiscountPercentage.Should().Be(20); // 10-20 items = 20% discount
        item.TotalAmount.Should().Be(1200); // 15 * 100 * 0.8
    }

    [Fact]
    public void AddItem_WithQuantityBelow4_ShouldNotApplyDiscount()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 3; // Should not get discount
        var unitPrice = 100m;

        // Act
        sale.AddItem(productId, productName, quantity, unitPrice);

        // Assert
        var item = sale.Items.First();
        item.DiscountPercentage.Should().Be(0); // No discount for < 4 items
        item.TotalAmount.Should().Be(300); // 3 * 100
    }

    [Fact]
    public void AddItem_WithQuantityAbove20_ShouldThrowException()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 25; // Above limit
        var unitPrice = 100m;

        // Act & Assert
        var act = () => sale.AddItem(productId, productName, quantity, unitPrice);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot sell more than 20 identical items");
    }

    [Fact]
    public void AddItem_SameProductMultipleTimes_ShouldCombineQuantities()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var unitPrice = 100m;

        // Act
        sale.AddItem(productId, productName, 3, unitPrice); // First addition
        sale.AddItem(productId, productName, 2, unitPrice); // Second addition = 5 total

        // Assert
        sale.Items.Should().HaveCount(1);
        var item = sale.Items.First();
        item.Quantity.Should().Be(5);
        item.DiscountPercentage.Should().Be(10); // 5 items = 10% discount
    }

    [Fact]
    public void AddItem_CombinedQuantityAbove20_ShouldThrowException()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var unitPrice = 100m;

        sale.AddItem(productId, productName, 15, unitPrice);

        // Act & Assert
        var act = () => sale.AddItem(productId, productName, 10, unitPrice); // 15 + 10 = 25
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot sell more than 20 identical items");
    }

    [Fact]
    public void UpdateItemQuantity_WithValidQuantity_ShouldUpdateItemAndDiscount()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Test Product", 5, 100m);
        var itemId = sale.Items.First().Id;

        // Act
        sale.UpdateItemQuantity(itemId, 12); // Change to 12 items

        // Assert
        var item = sale.Items.First();
        item.Quantity.Should().Be(12);
        item.DiscountPercentage.Should().Be(20); // 12 items = 20% discount
    }

    [Fact]
    public void UpdateItemQuantity_ToZero_ShouldCancelItem()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Test Product", 5, 100m);
        var itemId = sale.Items.First().Id;

        // Act
        sale.UpdateItemQuantity(itemId, 0);

        // Assert
        var item = sale.Items.First();
        item.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void CancelItem_ShouldSetItemAsCancelled()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Test Product", 5, 100m);
        var itemId = sale.Items.First().Id;

        // Act
        sale.CancelItem(itemId);

        // Assert
        var item = sale.Items.First();
        item.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(0); // Cancelled items don't count
    }

    [Fact]
    public void Cancel_ShouldSetSaleStatusToCancelled()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        sale.AddItem(Guid.NewGuid(), "Test Product", 5, 100m);

        // Act
        sale.Cancel();

        // Assert
        sale.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact]
    public void AddItem_ToCancelledSale_ShouldThrowException()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        sale.Cancel();

        // Act & Assert
        var act = () => sale.AddItem(Guid.NewGuid(), "Test Product", 5, 100m);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add items to a cancelled sale");
    }

    [Fact]
    public void IsValid_WithValidSale_ShouldReturnTrue()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        sale.AddItem(Guid.NewGuid(), "Test Product", 5, 100m);

        // Act
        var isValid = sale.IsValid(out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_WithNoItems_ShouldReturnFalse()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");

        // Act
        var isValid = sale.IsValid(out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Sale must have at least one active item");
    }

    [Fact]
    public void TotalAmount_ShouldExcludeCancelledItems()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer", "Branch");
        sale.AddItem(Guid.NewGuid(), "Product 1", 5, 100m); // 450 with discount
        sale.AddItem(Guid.NewGuid(), "Product 2", 3, 200m); // 600 no discount
        
        var firstItemId = sale.Items.First().Id;
        sale.CancelItem(firstItemId);

        // Act
        var total = sale.TotalAmount;

        // Assert
        total.Should().Be(600); // Only the second item
    }
}