using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMediation.Abstractions;
using OpenMediation.Dispatching;
using OpenMediation.Pipeline;
using System.Reflection;

namespace OpenMediation.DependencyInjection;



/// <summary>
/// Provides extension methods to register OpenMediation services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class MediationConfiguration
{
    /// <summary>
    /// Registers OpenMediation core services and scans the provided <paramref name="assemblies"/> for handlers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="assemblies">The assemblies to scan for <see cref="IRequestHandler{TRequest, TResponse}"/> and <see cref="INotificationHandler{TNotification}"/> implementations.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddOpenMediation(
        this IServiceCollection services,
        params ReadOnlySpan<Assembly> assemblies)
    {
        // Singletons: safe to share across scopes, survive test rebuilds.
        services.AddSingleton<IRequestExecutionPlanCache, RequestExecutionPlanCache>();
        services.AddSingleton<INotificationExecutionPlanCache, NotificationExecutionPlanCache>();

        // Scoped: participate in the ambient unit-of-work / DbContext lifecycle.
        services.AddScoped<ISender, RequestDispatcher>();
        services.AddScoped<IPublisher, NotificationPublisher>();
        services.AddScoped<IMediator, Mediator>();

        foreach (Assembly assembly in assemblies)
        {
            RegisterHandlers(services, assembly, typeof(IRequestHandler<,>));
            RegisterHandlers(services, assembly, typeof(INotificationHandler<>));
        }

        return services;
    }

    /// <summary>
    /// Registers an open-generic pipeline behavior.
    /// Behaviors execute in registration order (first-registered = outermost).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the behavior to.</param>
    /// <param name="behaviorType">The open-generic type of the behavior (e.g., typeof(LoggingBehavior&lt;,&gt;)).</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="behaviorType"/> is null.</exception>
    public static IServiceCollection AddOpenMediationBehavior(
        this IServiceCollection services,
        Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(behaviorType);
        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }

    /// <summary>
    /// Internal helper to register handlers from an assembly.
    /// Private methods do not require XML comments for the compiler.
    /// </summary>
    private static void RegisterHandlers(
        IServiceCollection services,
        Assembly assembly,
        Type openGenericHandlerType)
    {
        foreach (Type implementationType in assembly.GetTypes())
        {
            if (implementationType.IsAbstract
             || implementationType.IsInterface
             || implementationType.ContainsGenericParameters)
                continue;

            foreach (Type iface in implementationType.GetInterfaces())
            {
                if (!iface.IsGenericType
                 || iface.GetGenericTypeDefinition() != openGenericHandlerType)
                    continue;

                services.TryAddEnumerable(
                    ServiceDescriptor.Scoped(iface, implementationType));
            }
        }
    }
}