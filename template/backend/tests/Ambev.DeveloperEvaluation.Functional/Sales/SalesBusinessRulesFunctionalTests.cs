using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for Sales business rules through API
/// </summary>
public class SalesBusinessRulesFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SalesBusinessRulesFunctionalTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

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
    public async Task CreateSale_ShouldApplyCorrectDiscount_BasedOnQuantity(int quantity, decimal expectedDiscount)
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"DISCOUNT-TEST-{quantity}-{Guid.NewGuid():N}",
            Customer = "Discount Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = $"Product-{quantity}",
                    Quantity = quantity,
                    UnitPrice = 100.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseWithData<CreateSaleResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        
        var item = result.Data.Items.First();
        item.DiscountPercentage.Should().Be(expectedDiscount);
        item.Quantity.Should().Be(quantity);
    }

    [Fact]
    public async Task CreateSale_ShouldRejectRequest_WhenQuantityExceeds20()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"REJECT-TEST-{Guid.NewGuid():N}",
            Customer = "Reject Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Invalid Product",
                    Quantity = 21, // Exceeds maximum
                    UnitPrice = 100.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_ShouldCalculateCorrectTotalAmount_WithDiscounts()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"TOTAL-TEST-{Guid.NewGuid():N}",
            Customer = "Total Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 5, // 10% discount
                    UnitPrice = 100.00m // 5 * 100 * 0.9 = 450
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product B",
                    Quantity = 3, // No discount
                    UnitPrice = 50.00m // 3 * 50 = 150
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product C",
                    Quantity = 12, // 20% discount
                    UnitPrice = 25.00m // 12 * 25 * 0.8 = 240
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseWithData<CreateSaleResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
        
        // Check individual item calculations
        var productA = result.Data.Items.First(i => i.ProductName == "Product A");
        productA.DiscountPercentage.Should().Be(10m);
        productA.TotalAmount.Should().Be(450.00m);
        
        var productB = result.Data.Items.First(i => i.ProductName == "Product B");
        productB.DiscountPercentage.Should().Be(0m);
        productB.TotalAmount.Should().Be(150.00m);
        
        var productC = result.Data.Items.First(i => i.ProductName == "Product C");
        productC.DiscountPercentage.Should().Be(20m);
        productC.TotalAmount.Should().Be(240.00m);
        
        // Check total sale amount: 450 + 150 + 240 = 840
        result.Data.TotalAmount.Should().Be(840.00m);
    }

    [Fact]
    public async Task CreateSale_ShouldRequireAtLeastOneItem()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"EMPTY-TEST-{Guid.NewGuid():N}",
            Customer = "Empty Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>() // Empty items list
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_ShouldRejectZeroOrNegativeQuantity()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"ZERO-QTY-TEST-{Guid.NewGuid():N}",
            Customer = "Zero Quantity Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Invalid Product",
                    Quantity = 0, // Invalid quantity
                    UnitPrice = 100.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_ShouldRejectZeroOrNegativePrice()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"ZERO-PRICE-TEST-{Guid.NewGuid():N}",
            Customer = "Zero Price Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Invalid Product",
                    Quantity = 5,
                    UnitPrice = 0m // Invalid price
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_ShouldRejectEmptyRequiredFields()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = "", // Empty sale number
            Customer = "", // Empty customer
            Branch = "", // Empty branch
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Valid Product",
                    Quantity = 5,
                    UnitPrice = 100.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}