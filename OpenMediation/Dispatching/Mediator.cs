using OpenMediation.Abstractions;

namespace OpenMediation.Dispatching;

internal sealed class Mediator(ISender sender, IPublisher publisher) : IMediator
{
    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
        => sender.Send(request, cancellationToken);

    public Task Publish(
        INotification notification,
        CancellationToken cancellationToken = default)
        => publisher.Publish(notification, cancellationToken);
}