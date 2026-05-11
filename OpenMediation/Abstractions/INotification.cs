
namespace OpenMediation.Abstractions;


/// <summary>Marker interface for a notification (fan-out, no response).</summary>
public interface INotification;

/// <summary>Defines a handler for a notification of type <typeparamref name="TNotification"/>.</summary>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}