using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleRequest
/// </summary>
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    /// <summary>
    /// Initializes a new instance of CreateSaleRequestValidator
    /// </summary>
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.SaleNumber)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Sale number is required and must be at most 50 characters");

        RuleFor(x => x.Customer)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Customer is required and must be at most 200 characters");

        RuleFor(x => x.Branch)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Branch is required and must be at most 200 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Sale must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateSaleItemRequestValidator());
    }
}

/// <summary>
/// Validator for CreateSaleItemRequest
/// </summary>
public class CreateSaleItemRequestValidator : AbstractValidator<CreateSaleItemRequest>
{
    /// <summary>
    /// Initializes a new instance of CreateSaleItemRequestValidator
    /// </summary>
    public CreateSaleItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Product name is required and must be at most 200 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Quantity must be between 1 and 20");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than 0");
    }
}