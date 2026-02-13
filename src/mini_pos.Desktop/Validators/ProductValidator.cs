using FluentValidation;
using mini_pos.Models;

namespace mini_pos.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Barcode)
            .NotEmpty().WithMessage("Barcode is required")
            .MaximumLength(50).WithMessage("Barcode must not exceed 50 characters");

        RuleFor(p => p.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(p => p.Unit)
            .NotEmpty().WithMessage("Unit is required");

        RuleFor(p => p.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be 0 or greater");

        RuleFor(p => p.QuantityMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum quantity must be 0 or greater");

        RuleFor(p => p.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be 0 or greater");

        RuleFor(p => p.RetailPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Retail price must be 0 or greater")
            .GreaterThanOrEqualTo(p => p.CostPrice)
            .When(p => p.CostPrice > 0)
            .WithMessage("Retail price must be greater than or equal to cost price");

        RuleFor(p => p.BrandId)
            .NotEmpty().WithMessage("Brand is required");

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(p => p.Status)
            .NotEmpty().WithMessage("Status is required");
    }
}
