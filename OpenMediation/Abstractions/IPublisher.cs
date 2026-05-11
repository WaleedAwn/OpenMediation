
namespace OpenMediation.Abstractions;

public interface IPublisher
{
    Task Publish(
        INotification notification,
        CancellationToken cancellationToken = default);
}