using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for Sales API endpoints
/// </summary>
public class SalesControllerFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SalesControllerFunctionalTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateSale_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"FUNC-TEST-{Guid.NewGuid():N}",
            Customer = "Functional Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product A",
                    Quantity = 5,
                    UnitPrice = 100.00m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product B",
                    Quantity = 15,
                    UnitPrice = 50.00m
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
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SaleNumber.Should().Be(request.SaleNumber);
        result.Data.Items.Should().HaveCount(2);
        
        // Verify discount calculations
        var productA = result.Data.Items.First(i => i.ProductName == "Test Product A");
        productA.DiscountPercentage.Should().Be(10m); // 5 items = 10% discount
        
        var productB = result.Data.Items.First(i => i.ProductName == "Test Product B");
        productB.DiscountPercentage.Should().Be(20m); // 15 items = 20% discount
    }

    [Fact]
    public async Task CreateSale_ShouldReturnBadRequest_WhenQuantityExceeds20()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            SaleNumber = $"FUNC-TEST-INVALID-{Guid.NewGuid():N}",
            Customer = "Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Invalid Product",
                    Quantity = 25, // Exceeds limit
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
    public async Task GetSale_ShouldReturnSale_WhenExists()
    {
        // Arrange - First create a sale
        var createRequest = new CreateSaleRequest
        {
            SaleNumber = $"FUNC-GET-{Guid.NewGuid():N}",
            Customer = "Get Test Customer",
            Branch = "Get Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Get Test Product",
                    Quantity = 10,
                    UnitPrice = 75.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<ApiResponseWithData<CreateSaleResponse>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var saleId = createResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseWithData<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetSale_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sales/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSale_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange - First create a sale
        var createRequest = new CreateSaleRequest
        {
            SaleNumber = $"FUNC-UPDATE-{Guid.NewGuid():N}",
            Customer = "Original Customer",
            Branch = "Original Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Original Product",
                    Quantity = 5,
                    UnitPrice = 100.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<ApiResponseWithData<CreateSaleResponse>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var saleId = createResult!.Data!.Id;

        // Prepare update request
        var updateRequest = new UpdateSaleRequest
        {
            Customer = "Updated Customer",
            Branch = "Updated Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Updated Product",
                    Quantity = 12,
                    UnitPrice = 150.00m
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseWithData<UpdateSaleResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Customer.Should().Be("Updated Customer");
        result.Data.Branch.Should().Be("Updated Branch");
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().ProductName.Should().Be("Updated Product");
        result.Data.Items.First().DiscountPercentage.Should().Be(20m); // 12 items = 20% discount
    }

    [Fact]
    public async Task UpdateSale_ShouldReturnNotFound_WhenSaleNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateSaleRequest
        {
            Customer = "Test Customer",
            Branch = "Test Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 5,
                    UnitPrice = 100.00m
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sales/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelSale_ShouldReturnOk_WhenSaleExists()
    {
        // Arrange - First create a sale
        var createRequest = new CreateSaleRequest
        {
            SaleNumber = $"FUNC-CANCEL-{Guid.NewGuid():N}",
            Customer = "Cancel Test Customer",
            Branch = "Cancel Test Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Cancel Test Product",
                    Quantity = 7,
                    UnitPrice = 200.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<ApiResponseWithData<CreateSaleResponse>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var saleId = createResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetSales_ShouldReturnPaginatedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/sales?page=1&size=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseWithData<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }
}