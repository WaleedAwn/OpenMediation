
namespace OpenMediation.Abstractions;

/// <summary>
/// Marker interface for a request that produces a typed response.
/// </summary>
/// <typeparam name="TResponse">The type of the response produced by this request.</typeparam>
#pragma warning disable S2326
public interface IRequest<out TResponse>;
#pragma warning restore S2326

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response produced by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The incoming request object.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the handler response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}