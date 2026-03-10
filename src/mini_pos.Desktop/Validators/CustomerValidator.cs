using FluentValidation;

using mini_pos.Models;

namespace mini_pos.Validators;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(c => c.Surname)
            .NotEmpty().WithMessage("Surname is required")
            .MaximumLength(100).WithMessage("Surname must not exceed 100 characters");

        RuleFor(c => c.PhoneNumber)
            .Matches(@"^\d{8,15}$")
            .When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber))
            .WithMessage("Phone number must be 8-15 digits");

        RuleFor(c => c.Gender)
            .NotEmpty().WithMessage("Gender is required");
    }
}
