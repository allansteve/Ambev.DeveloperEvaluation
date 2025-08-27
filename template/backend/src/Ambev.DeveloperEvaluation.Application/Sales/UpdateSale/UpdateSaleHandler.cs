using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of UpdateSaleHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="logger">The logger</param>
    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateSaleCommand request
    /// </summary>
    /// <param name="command">The UpdateSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sale details</returns>
    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        // Validation is now handled by ValidationBehavior in MediatR pipeline

        // Get existing sale
        var existingSale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingSale == null)
            throw new InvalidOperationException($"Sale with ID {command.Id} not found");

        if (existingSale.Status == Domain.Enums.SaleStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale");

        // Update basic properties
        existingSale.Customer = command.Customer;
        existingSale.Branch = command.Branch;

        // Clear existing items and add new ones
        var itemsToRemove = existingSale.Items.Where(i => !command.Items.Any(ci => ci.Id == i.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            existingSale.RemoveItem(item.Id);
        }

        // Update existing items and add new ones
        foreach (var itemCommand in command.Items)
        {
            if (itemCommand.Id.HasValue)
            {
                // Update existing item
                try
                {
                    existingSale.UpdateItemQuantity(itemCommand.Id.Value, itemCommand.Quantity);
                }
                catch (InvalidOperationException)
                {
                    // If item doesn't exist, add as new
                    existingSale.AddItem(itemCommand.ProductId, itemCommand.ProductName, 
                        itemCommand.Quantity, itemCommand.UnitPrice);
                }
            }
            else
            {
                // Add new item
                try
                {
                    existingSale.AddItem(itemCommand.ProductId, itemCommand.ProductName, 
                        itemCommand.Quantity, itemCommand.UnitPrice);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException($"Item {itemCommand.ProductName}: {ex.Message}");
                }
            }
        }

        // Validate the complete sale
        if (!existingSale.IsValid(out var errors))
        {
            throw new InvalidOperationException(string.Join("; ", errors));
        }

        // Save the updated sale
        var updatedSale = await _saleRepository.UpdateAsync(existingSale, cancellationToken);

        // Log domain events
        foreach (var domainEvent in updatedSale.DomainEvents)
        {
            _logger.LogInformation("Domain event: {EventType} - {Event}", 
                domainEvent.GetType().Name, 
                System.Text.Json.JsonSerializer.Serialize(domainEvent));
        }

        // Clear domain events after logging
        updatedSale.ClearDomainEvents();

        var result = _mapper.Map<UpdateSaleResult>(updatedSale);
        return result;
    }
}