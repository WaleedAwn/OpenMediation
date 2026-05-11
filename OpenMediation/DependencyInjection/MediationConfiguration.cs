using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMediation.Abstractions;
using OpenMediation.Dispatching;
using OpenMediation.Pipeline;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpenMediation.DependencyInjection;

public static class MediationConfiguration
{
    public static IServiceCollection AddOpenMediation(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSingleton<IRequestExecutionPlanCache, RequestExecutionPlanCache>();
        services.AddScoped<ISender, RequestDispatcher>();
        services.AddScoped<IPublisher, NotificationPublisher>();
        services.AddScoped<IMediator, Mediator>();

        RegisterHandlers(services, assemblies, typeof(IRequestHandler<,>));
        RegisterHandlers(services, assemblies, typeof(INotificationHandler<>));

        return services;
    }

    public static IServiceCollection AddOpenMediationBehavior(this IServiceCollection services, Type behaviorType)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies, Type openGenericHandlerType)
    {
        var concreteTypes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.ContainsGenericParameters);

        foreach (var implementationType in concreteTypes)
        {
            var serviceTypes = implementationType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericHandlerType);

            foreach (var serviceType in serviceTypes)
            {
                services.TryAddEnumerable(ServiceDescriptor.Scoped(serviceType, implementationType));
            }
        }
    }
}
