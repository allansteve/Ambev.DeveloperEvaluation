using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Controller for managing sales operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of SalesController
    /// </summary>
    /// <param name="mediator">The mediator instance</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale
    /// </summary>
    /// <param name="request">The sale creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = _mapper.Map<CreateSaleCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);
            var response = _mapper.Map<CreateSaleResponse>(result);

            return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
            {
                Success = true,
                Message = "Sale created successfully",
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves a sale by ID
    /// </summary>
    /// <param name="id">The sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSaleQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = $"Sale with ID {id} not found"
            });
        }

        var response = _mapper.Map<GetSaleResponse>(result);
        return Ok(new ApiResponseWithData<GetSaleResponse>
        {
            Success = true,
            Message = "Sale retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Retrieves all sales with optional filtering and pagination
    /// </summary>
    /// <param name="page">Page number (starts from 1)</param>
    /// <param name="size">Page size</param>
    /// <param name="customer">Customer filter</param>
    /// <param name="branch">Branch filter</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of sales</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSalesResult>), 200)]
    public async Task<IActionResult> GetSales(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? customer = null,
        [FromQuery] string? branch = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesQuery
        {
            Page = page,
            Size = Math.Min(size, 100), // Limit page size to 100
            Customer = customer,
            Branch = branch,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponseWithData<GetSalesResult>
        {
            Success = true,
            Message = "Sales retrieved successfully",
            Data = result
        });
    }

    /// <summary>
    /// Cancels a sale
    /// </summary>
    /// <param name="id">The sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelSaleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = result.Message
            });
        }

        return Ok(new ApiResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Updates an existing sale
    /// </summary>
    /// <param name="id">The sale ID</param>
    /// <param name="request">The sale update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sale details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = _mapper.Map<UpdateSaleCommand>(request);
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            var response = _mapper.Map<UpdateSaleResponse>(result);

            return Ok(new ApiResponseWithData<UpdateSaleResponse>
            {
                Success = true,
                Message = "Sale updated successfully",
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }

            return Conflict(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}