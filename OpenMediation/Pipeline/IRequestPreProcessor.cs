namespace OpenMediation.Pipeline;

/// <summary>
/// A lightweight pre-processor that runs before the behavior pipeline.
/// Prefer <see cref="IPipelineBehavior{TRequest,TResponse}"/> when you need
/// access to the response or need to short-circuit the pipeline.
/// </summary>
public interface IRequestPreProcessor<in TRequest>
{
    Task Process(TRequest request, CancellationToken cancellationToken);
}