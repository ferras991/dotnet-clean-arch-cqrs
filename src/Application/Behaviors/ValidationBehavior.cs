using Application.Abstractions;
using Domain;
using Domain.Errors;
using FluentValidation;
using System.Reflection;

namespace Application.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => new ValidationFailure(f.PropertyName, f.ErrorMessage))
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var error = new ValidationError(failures);

        // TResponse is Result<T> — extract T and call Result.Failure<T>(error)
        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(Result.Failure) 
                             && m.IsGenericMethodDefinition
                             && m.GetParameters().Length == 1
                             && m.GetParameters()[0].ParameterType == typeof(DomainError))
                .MakeGenericMethod(innerType);

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        // TResponse is Result (non-generic)
        return (TResponse)(object)Result.Failure(error);
    }
}