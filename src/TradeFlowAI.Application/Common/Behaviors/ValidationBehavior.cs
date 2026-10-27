using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;

namespace TradeFlowAI.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior — Validation.
/// Runs all FluentValidation validators for a request before the handler.
/// Throws ValidationException if any rule fails.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new AppValidationException(failures);

        return await next();
    }
}
