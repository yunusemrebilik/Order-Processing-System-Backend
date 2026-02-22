using FluentValidation;

namespace ECommerce.Application.Products.UpdateProductPrice;

public class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
