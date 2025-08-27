using Xunit;
using FluentAssertions;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Tests for SaleItem entity business rules and discount calculations
/// </summary>
public class SaleItemTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 10)]
    [InlineData(5, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(15, 20)]
    [InlineData(20, 20)]
    public void CalculateDiscount_ShouldReturnCorrectDiscountPercentage(int quantity, decimal expectedDiscount)
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = quantity,
            UnitPrice = 100m
        };

        // Act
        var discount = saleItem.CalculateDiscount();

        // Assert
        discount.Should().Be(expectedDiscount);
    }

    [Fact]
    public void ApplyDiscount_ShouldSetCorrectDiscountPercentage()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 12,
            UnitPrice = 100m
        };

        // Act
        saleItem.ApplyDiscount();

        // Assert
        saleItem.DiscountPercentage.Should().Be(20);
    }

    [Theory]
    [InlineData(5, 100, 10, 450)]  // 5 * 100 * 0.9
    [InlineData(15, 50, 20, 600)]  // 15 * 50 * 0.8
    [InlineData(3, 200, 0, 600)]   // 3 * 200 * 1.0
    public void TotalAmount_ShouldCalculateCorrectly(int quantity, decimal unitPrice, decimal discountPercentage, decimal expectedTotal)
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercentage = discountPercentage
        };

        // Act
        var totalAmount = saleItem.TotalAmount;

        // Assert
        totalAmount.Should().Be(expectedTotal);
    }

    [Fact]
    public void IsValid_WithValidItem_ShouldReturnTrue()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 5,
            UnitPrice = 100m,
            DiscountPercentage = 10
        };

        // Act
        var isValid = saleItem.IsValid(out var errorMessage);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_WithZeroQuantity_ShouldReturnFalse()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 0,
            UnitPrice = 100m
        };

        // Act
        var isValid = saleItem.IsValid(out var errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Quantity must be greater than 0");
    }

    [Fact]
    public void IsValid_WithQuantityAbove20_ShouldReturnFalse()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 25,
            UnitPrice = 100m
        };

        // Act
        var isValid = saleItem.IsValid(out var errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Cannot sell more than 20 identical items");
    }

    [Fact]
    public void IsValid_WithZeroUnitPrice_ShouldReturnFalse()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 5,
            UnitPrice = 0m
        };

        // Act
        var isValid = saleItem.IsValid(out var errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Unit price must be greater than 0");
    }

    [Fact]
    public void IsValid_WithDiscountOnLowQuantity_ShouldReturnFalse()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 3,
            UnitPrice = 100m,
            DiscountPercentage = 10
        };

        // Act
        var isValid = saleItem.IsValid(out var errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Purchases below 4 items cannot have a discount");
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledToTrue()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 5,
            UnitPrice = 100m,
            IsCancelled = false
        };

        // Act
        saleItem.Cancel();

        // Assert
        saleItem.IsCancelled.Should().BeTrue();
    }
}