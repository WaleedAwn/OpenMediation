using OpenMediation.Abstractions;
using Microsoft.Extensions.DependencyInjection;


namespace OpenMediation.Dispatching;

public sealed class NotificationPublisher : IPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
        var handlers = _serviceProvider.GetServices(handlerType).Cast<object>().ToList();

        foreach (var handler in handlers)
        {
            await ((dynamic)handler).Handle((dynamic)notification, cancellationToken);
        }
    }
}