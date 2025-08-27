using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Handler for processing CancelSaleCommand requests
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of CancelSaleHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="logger">The logger</param>
    public CancelSaleHandler(ISaleRepository saleRepository, ILogger<CancelSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CancelSaleCommand request
    /// </summary>
    /// <param name="command">The CancelSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cancellation result</returns>
    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        
        if (sale == null)
        {
            return new CancelSaleResult
            {
                Id = command.Id,
                Success = false,
                Message = "Sale not found"
            };
        }

        try
        {
            sale.Cancel();
            await _saleRepository.UpdateAsync(sale, cancellationToken);

            // Log domain events
            foreach (var domainEvent in sale.DomainEvents)
            {
                _logger.LogInformation("Domain event: {EventType} - {Event}", 
                    domainEvent.GetType().Name, 
                    System.Text.Json.JsonSerializer.Serialize(domainEvent));
            }

            // Clear domain events after logging
            sale.ClearDomainEvents();

            return new CancelSaleResult
            {
                Id = sale.Id,
                SaleNumber = sale.SaleNumber,
                Success = true,
                Message = "Sale cancelled successfully"
            };
        }
        catch (InvalidOperationException ex)
        {
            return new CancelSaleResult
            {
                Id = command.Id,
                SaleNumber = sale.SaleNumber,
                Success = false,
                Message = ex.Message
            };
        }
    }
}