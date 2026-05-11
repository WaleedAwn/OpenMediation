using OpenMediation.Abstractions;

namespace OpenMediation.Dispatching;

/// <summary>
/// The default mediator implementation that coordinates requests and notifications.
/// </summary>
public sealed class Mediator(ISender sender, IPublisher publisher) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
        => sender.Send(request, ct);

    public Task Publish(INotification notification, CancellationToken ct = default)
        => publisher.Publish(notification, ct);
}