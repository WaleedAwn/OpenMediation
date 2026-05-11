using OpenMediation.Abstractions;
using Microsoft.Extensions.DependencyInjection;


namespace OpenMediation.Dispatching;

internal sealed class NotificationPublisher(
    IServiceProvider serviceProvider,
    INotificationExecutionPlanCache executionPlanCache) : IPublisher
{
    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        NotificationExecutionPlan plan = executionPlanCache.GetOrAdd(notification.GetType());

        object[] handlers = [.. serviceProvider.GetServices(plan.HandlerServiceType)];

        if (handlers.Length is 0) return;

        UntypedNotificationHandlerInvoker invoker = plan.HandlerInvoker;
        object concreteNotif = notification;

        foreach (object handler in handlers)
        {
            await invoker(handler, concreteNotif, cancellationToken).ConfigureAwait(false);
        }
    }
}