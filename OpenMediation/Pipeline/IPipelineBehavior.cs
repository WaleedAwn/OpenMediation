using OpenMediation.Abstractions;

namespace OpenMediation.Pipeline;

// <summary>
/// A delegate that, when invoked, calls the next step in the pipeline
/// (either the next behavior or the final handler).
/// CancellationToken is passed through so behaviors can substitute
/// a linked/timeout token for downstream stages.
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Represents a middleware behavior that wraps a request/response pair in the pipeline.
/// Behaviors are executed in registration order (first-registered = outermost).
/// </summary>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}

