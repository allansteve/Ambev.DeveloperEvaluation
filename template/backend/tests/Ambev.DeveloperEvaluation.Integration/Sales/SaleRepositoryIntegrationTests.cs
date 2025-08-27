using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// Integration tests for SaleRepository
/// </summary>
public class SaleRepositoryIntegrationTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly SaleRepository _repository;

    public SaleRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _repository = new SaleRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistSaleWithItems()
    {
        // Arrange
        var sale = Sale.Create("SALE-001", "Customer Test", "Branch Test");
        sale.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        sale.AddItem(Guid.NewGuid(), "Product B", 15, 20.00m);

        // Act
        var result = await _repository.CreateAsync(sale, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sale.SaleNumber, result.SaleNumber);
        Assert.Equal(2, result.Items.Count);
        
        var productA = result.Items.First(i => i.ProductName == "Product A");
        Assert.Equal(10m, productA.DiscountPercentage); // 5 items = 10% discount
        
        var productB = result.Items.First(i => i.ProductName == "Product B");
        Assert.Equal(20m, productB.DiscountPercentage); // 15 items = 20% discount
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSaleWithItems()
    {
        // Arrange
        var sale = Sale.Create("SALE-002", "Customer Test 2", "Branch Test 2");
        sale.AddItem(Guid.NewGuid(), "Product C", 3, 15.00m);
        await _repository.CreateAsync(sale, CancellationToken.None);

        // Act
        var result = await _repository.GetByIdAsync(sale.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sale.SaleNumber, result.SaleNumber);
        Assert.Single(result.Items);
        Assert.Equal("Product C", result.Items.First().ProductName);
        Assert.Equal(0m, result.Items.First().DiscountPercentage); // 3 items = no discount
    }

    [Fact]
    public async Task GetBySaleNumberAsync_ShouldReturnCorrectSale()
    {
        // Arrange
        var sale = Sale.Create("SALE-003", "Customer Test 3", "Branch Test 3");
        sale.AddItem(Guid.NewGuid(), "Product D", 10, 25.00m);
        await _repository.CreateAsync(sale, CancellationToken.None);

        // Act
        var result = await _repository.GetBySaleNumberAsync("SALE-003", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SALE-003", result.SaleNumber);
        Assert.Equal("Customer Test 3", result.Customer);
        Assert.Single(result.Items);
        Assert.Equal(20m, result.Items.First().DiscountPercentage); // 10 items = 20% discount
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var sale = Sale.Create("SALE-004", "Customer Test 4", "Branch Test 4");
        sale.AddItem(Guid.NewGuid(), "Product E", 8, 30.00m);
        await _repository.CreateAsync(sale, CancellationToken.None);

        // Act
        sale.Customer = "Updated Customer";
        sale.AddItem(Guid.NewGuid(), "Product F", 12, 40.00m);
        var result = await _repository.UpdateAsync(sale, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Customer", result.Customer);
        Assert.Equal(2, result.Items.Count);
        
        var productF = result.Items.First(i => i.ProductName == "Product F");
        Assert.Equal(20m, productF.DiscountPercentage); // 12 items = 20% discount
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSale()
    {
        // Arrange
        var sale = Sale.Create("SALE-005", "Customer Test 5", "Branch Test 5");
        sale.AddItem(Guid.NewGuid(), "Product G", 6, 35.00m);
        await _repository.CreateAsync(sale, CancellationToken.None);

        // Act
        await _repository.DeleteAsync(sale.Id, CancellationToken.None);
        var result = await _repository.GetByIdAsync(sale.Id, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFilteredResults()
    {
        // Arrange
        var sale1 = Sale.Create("SALE-006", "Customer A", "Branch 1");
        sale1.AddItem(Guid.NewGuid(), "Product H", 4, 10.00m);
        await _repository.CreateAsync(sale1, CancellationToken.None);

        var sale2 = Sale.Create("SALE-007", "Customer B", "Branch 2");
        sale2.AddItem(Guid.NewGuid(), "Product I", 7, 15.00m);
        await _repository.CreateAsync(sale2, CancellationToken.None);

        // Act
        var allSales = await _repository.GetAllAsync(0, 10, CancellationToken.None);
        var filteredByCustomer = await _repository.GetByCustomerAsync("Customer A", CancellationToken.None);

        // Assert
        Assert.True(allSales.Count() >= 2);
        Assert.Single(filteredByCustomer);
        Assert.Equal("Customer A", filteredByCustomer.First().Customer);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}