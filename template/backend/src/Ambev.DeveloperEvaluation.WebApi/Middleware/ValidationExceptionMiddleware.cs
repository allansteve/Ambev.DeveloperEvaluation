using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.Common.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware to process all exceptions and return appropriate HTTP responses.
/// </summary>
public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

    public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        ApiResponse response;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response = new ApiResponse
                {
                    Success = false,
                    Message = "One or more validation errors occurred.",
                    Errors = validationException.Errors.Select(error => (ValidationErrorDetail)error)
                };
                break;

            case InvalidOperationException:
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                response = new ApiResponse
                {
                    Success = false,
                    Message = exception.Message
                };
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = new ApiResponse
                {
                    Success = false,
                    Message = "The requested resource was not found."
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                };
                _logger.LogError(exception, "An unhandled exception has occurred: {Message}", exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
