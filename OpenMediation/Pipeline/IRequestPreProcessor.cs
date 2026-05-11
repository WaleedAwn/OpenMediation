namespace OpenMediation.Pipeline;

/// <summary>
/// A lightweight pre-processor that runs before the behavior pipeline.
/// Prefer <see cref="IPipelineBehavior{TRequest,TResponse}"/> when you need
/// access to the response or need to short-circuit the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request being processed.</typeparam>
public interface IRequestPreProcessor<in TRequest>
{
    /// <summary>
    /// Performs pre-processing logic on the specified request.
    /// </summary>
    /// <param name="request">The incoming request object.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous pre-processing operation.</returns>
    Task Process(TRequest request, CancellationToken cancellationToken);
}