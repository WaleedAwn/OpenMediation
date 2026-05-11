using OpenMediation.Abstractions;

namespace OpenMediation.Pipeline;

/// <summary>
/// A delegate that, when invoked, calls the next step in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the next operation in the pipeline.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Represents a middleware behavior that wraps a request/response pair in the pipeline.
/// Behaviors are executed in registration order.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the pipeline step for the specified request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next delegate in the pipeline chain.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response produced by the handler or next behavior.</returns>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
