using Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        var behaviors = serviceProvider
            .GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)))
            .Cast<dynamic>()
            .Reverse()
            .ToList();

        RequestHandlerDelegate<TResponse> pipeline = ct =>
        {
            var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;
            return (Task<TResponse>)method.Invoke(handler, [request, ct])!;
        };

        pipeline = behaviors.Aggregate(pipeline, (next, behavior) =>
            ct => behavior.Handle((dynamic)request, next, ct));

        return await pipeline(cancellationToken);
    }
}