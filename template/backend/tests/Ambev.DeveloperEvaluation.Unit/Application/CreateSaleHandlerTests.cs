using Xunit;
using FluentAssertions;
using NSubstitute;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Tests for CreateSaleHandler to ensure proper command handling and business rule validation
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSaleSuccessfully()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            Customer = "Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 5,
                    UnitPrice = 100m
                }
            }
        };

        var createdSale = Sale.Create(command.SaleNumber, command.Customer, command.Branch);
        createdSale.AddItem(command.Items[0].ProductId, command.Items[0].ProductName, 
                           command.Items[0].Quantity, command.Items[0].UnitPrice);

        var expectedResult = new CreateSaleResult
        {
            Id = createdSale.Id,
            SaleNumber = command.SaleNumber,
            Customer = command.Customer,
            Branch = command.Branch,
            TotalAmount = createdSale.TotalAmount
        };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(createdSale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.Customer.Should().Be(command.Customer);
        result.Branch.Should().Be(command.Branch);

        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowInvalidOperationException()
    {
        // Arrange - Note: Basic validation is now handled by ValidationBehavior in MediatR pipeline
        // This test focuses on domain validation that happens in the handler
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            Customer = "Test Customer", 
            Branch = "Test Branch",
            Items = new List<CreateSaleItemCommand>() // Empty items - will cause domain validation error
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*at least one active item*");
    }

    [Fact]
    public async Task Handle_WithExistingSaleNumber_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            Customer = "Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 5,
                    UnitPrice = 100m
                }
            }
        };

        var existingSale = Sale.Create(command.SaleNumber, "Other Customer", "Other Branch");
        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(existingSale);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Sale with number {command.SaleNumber} already exists");
    }

    [Fact]
    public async Task Handle_WithItemQuantityAbove20_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            Customer = "Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 25, // Above limit
                    UnitPrice = 100m
                }
            }
        };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot sell more than 20 identical items*");
    }

    [Fact]
    public async Task Handle_ShouldClearDomainEventsAfterLogging()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            Customer = "Test Customer",
            Branch = "Test Branch",
            Items = new List<CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 5,
                    UnitPrice = 100m
                }
            }
        };

        var createdSale = Sale.Create(command.SaleNumber, command.Customer, command.Branch);
        createdSale.AddItem(command.Items[0].ProductId, command.Items[0].ProductName, 
                           command.Items[0].Quantity, command.Items[0].UnitPrice);

        var expectedResult = new CreateSaleResult
        {
            Id = createdSale.Id,
            SaleNumber = command.SaleNumber
        };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(createdSale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>())
            .Returns(expectedResult);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Domain events should be cleared after processing
        createdSale.DomainEvents.Should().BeEmpty();
    }
}