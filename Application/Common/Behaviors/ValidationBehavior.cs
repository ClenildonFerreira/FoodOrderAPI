using FluentValidation;
using MediatR;

namespace FoodOrderApi.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
            return await next();
            
            var context = new ValidationContext<TRequest>(request);

            var erros = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();
        if (erros.Any())
        {
        var errorMassage = string.Join("; ", erros.Select(e => e.ErrorMessage));
        throw new ValidationException(errorMassage);
        }
        return await next();
    }
}

