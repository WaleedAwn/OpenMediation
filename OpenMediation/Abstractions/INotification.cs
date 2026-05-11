
namespace OpenMediation.Abstractions;

/// <summary>
/// Marker interface for a notification (fan-out, no response).
/// </summary>
public interface INotification;

/// <summary>
/// Defines a handler for a notification of type <typeparamref name="TNotification"/>.
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification instance to handle.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}