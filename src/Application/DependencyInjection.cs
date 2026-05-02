using Application.Abstractions;
using Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<IDispatcher, Dispatcher>();

        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));

        // ValidationBehavior must run before LoggingBehavior wraps the handler
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type openHandlerType)
    {
        var handlers = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (impl, iface) => (impl, iface))
            .Where(x => x.iface.IsGenericType &&
                        x.iface.GetGenericTypeDefinition() == openHandlerType);

        foreach (var (impl, iface) in handlers)
        {
            services.AddScoped(iface, impl);

            // Also register as IRequestHandler<,> so the Dispatcher can resolve it
            var requestHandlerType = iface.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                     i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            if (requestHandlerType is not null)
                services.AddScoped(requestHandlerType, impl);
        }
    }
}