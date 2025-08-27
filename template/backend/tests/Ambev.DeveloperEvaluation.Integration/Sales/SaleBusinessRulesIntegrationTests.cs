using Ambev.DeveloperEvaluation.Domain.Entities;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// Integration tests for Sale business rules
/// </summary>
public class SaleBusinessRulesIntegrationTests
{
    [Theory]
    [InlineData(1, 0)] // Below 4 items - no discount
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 10)] // 4-9 items - 10% discount
    [InlineData(5, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)] // 10-20 items - 20% discount
    [InlineData(15, 20)]
    [InlineData(20, 20)]
    public void AddItem_ShouldApplyCorrectDiscountBasedOnQuantity(int quantity, decimal expectedDiscount)
    {
        // Arrange
        var sale = Sale.Create("SALE-DISCOUNT-TEST", "Customer", "Branch");
        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "Test Product", quantity, 100.00m);

        // Assert
        var item = sale.Items.First();
        Assert.Equal(expectedDiscount, item.DiscountPercentage);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenQuantityExceeds20()
    {
        // Arrange
        var sale = Sale.Create("SALE-MAX-QUANTITY", "Customer", "Branch");
        var productId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            sale.AddItem(productId, "Test Product", 21, 100.00m));
        
        Assert.Contains("Cannot sell more than 20 identical items", exception.Message);
    }

    [Fact]
    public void AddItem_ShouldCombineQuantities_WhenAddingSameProduct()
    {
        // Arrange
        var sale = Sale.Create("SALE-COMBINE", "Customer", "Branch");
        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "Test Product", 5, 100.00m); // 10% discount
        sale.AddItem(productId, "Test Product", 7, 100.00m); // Total 12 = 20% discount

        // Assert
        Assert.Single(sale.Items);
        var item = sale.Items.First();
        Assert.Equal(12, item.Quantity);
        Assert.Equal(20m, item.DiscountPercentage); // 12 items = 20% discount
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenCombinedQuantityExceeds20()
    {
        // Arrange
        var sale = Sale.Create("SALE-COMBINE-MAX", "Customer", "Branch");
        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "Test Product", 15, 100.00m);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            sale.AddItem(productId, "Test Product", 6, 100.00m)); // 15 + 6 = 21

        Assert.Contains("Cannot sell more than 20 identical items", exception.Message);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldApplyCorrectDiscount()
    {
        // Arrange
        var sale = Sale.Create("SALE-UPDATE", "Customer", "Branch");
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Test Product", 5, 100.00m); // 10% discount
        var itemId = sale.Items.First().Id;

        // Act
        sale.UpdateItemQuantity(itemId, 15); // Change to 20% discount

        // Assert
        var item = sale.Items.First();
        Assert.Equal(15, item.Quantity);
        Assert.Equal(20m, item.DiscountPercentage);
    }

    [Fact]
    public void CancelItem_ShouldNotAffectTotalAmount()
    {
        // Arrange
        var sale = Sale.Create("SALE-CANCEL-ITEM", "Customer", "Branch");
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        
        sale.AddItem(product1Id, "Product 1", 5, 100.00m); // 10% discount = 450.00
        sale.AddItem(product2Id, "Product 2", 3, 50.00m);  // No discount = 150.00
        
        var initialTotal = sale.TotalAmount; // 600.00
        var itemToCancel = sale.Items.First(i => i.ProductName == "Product 1");

        // Act
        sale.CancelItem(itemToCancel.Id);

        // Assert
        Assert.Equal(150.00m, sale.TotalAmount); // Only Product 2 remains
        Assert.True(sale.Items.First(i => i.ProductName == "Product 1").IsCancelled);
        Assert.False(sale.Items.First(i => i.ProductName == "Product 2").IsCancelled);
    }

    [Fact]
    public void Cancel_ShouldPreventFurtherModifications()
    {
        // Arrange
        var sale = Sale.Create("SALE-CANCEL", "Customer", "Branch");
        sale.AddItem(Guid.NewGuid(), "Test Product", 5, 100.00m);

        // Act
        sale.Cancel();

        // Assert
        Assert.Equal(Domain.Enums.SaleStatus.Cancelled, sale.Status);
        
        var exception = Assert.Throws<InvalidOperationException>(() =>
            sale.AddItem(Guid.NewGuid(), "Another Product", 3, 50.00m));
        
        Assert.Contains("Cannot add items to a cancelled sale", exception.Message);
    }
}