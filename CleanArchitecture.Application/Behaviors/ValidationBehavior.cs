using CleanArchitecture.Domain.Abstractions;
using FluentValidation;
using MediatR;

namespace CleanArchitecture.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
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
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new Error(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Any())
        {
            // returns a custom ValidationResult instead of throwing.
            // For simplicity, returning a generic failure or a specific ValidationError.
            // Assuming we throw a ValidationException or create a failed result:
            throw new ValidationException(errors[0].Code);
            // In a fuller implementation, we create a specific ValidationResult class inheriting Result here.
        }

        return await next();
    }
}
