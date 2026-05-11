using OpenMediation.Abstractions;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenMediation.Dispatching;

// AOT: Expression.Compile() is not AOT-safe. Replace BuildHandlerInvoker with a
// source-generated switch over known INotification types when targeting PublishAot=true.

internal sealed class NotificationExecutionPlanCache : INotificationExecutionPlanCache
{
    private static readonly ConcurrentDictionary<Type, NotificationExecutionPlan> s_cache = new();

    public NotificationExecutionPlan GetOrAdd(Type notificationType)
        => s_cache.GetOrAdd(notificationType, static t => BuildExecutionPlan(t));

    private static NotificationExecutionPlan BuildExecutionPlan(Type notificationType)
    {
        Type handlerServiceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        return new NotificationExecutionPlan(
            handlerServiceType,
            BuildHandlerInvoker(notificationType, handlerServiceType));
    }

    private static UntypedNotificationHandlerInvoker BuildHandlerInvoker(
        Type notificationType, Type handlerServiceType)
    {
        MethodInfo handleMethod = handlerServiceType.GetMethod(
            "Handle", [notificationType, typeof(CancellationToken)])!;

        ParameterExpression handlerParam = Expression.Parameter(typeof(object), "handler");
        ParameterExpression notificationParam = Expression.Parameter(typeof(object), "notification");
        ParameterExpression tokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        Expression body = Expression.Call(
            Expression.Convert(handlerParam, handlerServiceType),
            handleMethod,
            Expression.Convert(notificationParam, notificationType),
            tokenParam);

        return Expression.Lambda<UntypedNotificationHandlerInvoker>(
                   body, handlerParam, notificationParam, tokenParam)
               .Compile();
    }
}