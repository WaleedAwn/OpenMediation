
namespace OpenMediation.Abstractions;

/// <summary>
/// Publishes a notification to zero or more handlers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Asynchronously publishes a notification to all registered handlers for the notification type.
    /// </summary>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}