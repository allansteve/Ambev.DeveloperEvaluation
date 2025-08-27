using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand
/// </summary>
public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of CreateSaleValidator
    /// </summary>
    public CreateSaleValidator()
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
            .SetValidator(new CreateSaleItemValidator());
    }
}

/// <summary>
/// Validator for CreateSaleItemCommand
/// </summary>
public class CreateSaleItemValidator : AbstractValidator<CreateSaleItemCommand>
{
    /// <summary>
    /// Initializes a new instance of CreateSaleItemValidator
    /// </summary>
    public CreateSaleItemValidator()
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