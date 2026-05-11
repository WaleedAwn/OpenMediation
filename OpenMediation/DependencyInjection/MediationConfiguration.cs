using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMediation.Abstractions;
using OpenMediation.Dispatching;
using OpenMediation.Pipeline;
using System.Reflection;

namespace OpenMediation.DependencyInjection;

public static class MediationConfiguration
{
    /// <summary>
    /// Registers OpenMediation and scans <paramref name="assemblies"/> for handlers.
    /// Usage: services.AddOpenMediation(typeof(MyHandler).Assembly)
    /// </summary>
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
    /// Example: services.AddOpenMediationBehavior(typeof(LoggingBehavior&lt;,&gt;))
    /// </summary>
    public static IServiceCollection AddOpenMediationBehavior(
        this IServiceCollection services,
        Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(behaviorType);
        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }

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