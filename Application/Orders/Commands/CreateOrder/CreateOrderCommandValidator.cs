using FluentValidation;

namespace FoodOrderAPI.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
       RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do cliente não pode exceder 100 caracteres.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Tipo de pedido inválido.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("O pedido deve conter pelo menos 1 item.");

            RuleForEach(x => x.Items).ChildRules(items =>
            {
                items.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");
            });
    }
}