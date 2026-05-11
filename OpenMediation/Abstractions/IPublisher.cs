
namespace OpenMediation.Abstractions;

/// <summary>Publishes a notification to zero or more handlers.</summary>
public interface IPublisher
{
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}