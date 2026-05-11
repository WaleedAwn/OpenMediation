using OpenMediation.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace OpenMediation.Dispatching;

/// <summary>
/// Dispatches requests to their corresponding handlers through the configured pipeline behaviors.
/// This implementation uses a cached execution-plan model:
/// it caches immutable invocation metadata and compiled delegates,
/// but always resolves handlers and behaviors from the current DI scope.
/// </summary>
public sealed class RequestDispatcher : ISender
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRequestExecutionPlanCache _executionPlanCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider used to resolve handlers and behaviors.
    /// </param>
    /// <param name="executionPlanCache">
    /// The cache that stores immutable execution plans for request/response pairs.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any dependency is <see langword="null"/>.
    /// </exception>
    public RequestDispatcher(
            IServiceProvider serviceProvider,
            IRequestExecutionPlanCache executionPlanCache)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _executionPlanCache = executionPlanCache ?? throw new ArgumentNullException(nameof(executionPlanCache));
    }

    /// <summary>
    /// Sends a request through the pipeline and returns its response.
    /// A request must have exactly one registered handler.
    /// Pipeline behaviors are executed in registration order, with the first registered
    /// behavior becoming the outermost behavior in the pipeline.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response produced by the request handler.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler or multiple handlers are registered for the request.
    /// </exception>
    public async Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);

        RequestExecutionPlan executionPlan = _executionPlanCache.GetOrAdd(requestType, responseType);

        object[] handlers = _serviceProvider
                .GetServices(executionPlan.HandlerServiceType)
                .Cast<object>()
                .ToArray();

        if (handlers.Length == 0)
        {
            throw new InvalidOperationException(
                    $"No request handler is registered for request type '{requestType.FullName}' " +
                    $"with response type '{responseType.FullName}'.");
        }

        if (handlers.Length > 1)
        {
            throw new InvalidOperationException(
                    $"Multiple request handlers are registered for request type '{requestType.FullName}' " +
                    $"with response type '{responseType.FullName}'. Exactly one handler is required.");
        }

        object handler = handlers[0];

        object[] behaviors = _serviceProvider
                .GetServices(executionPlan.BehaviorServiceType)
                .Cast<object>()
                .ToArray();

        Func<Task<object?>> pipeline = () =>
                executionPlan.HandlerInvoker(handler, request, cancellationToken);

        foreach (object behavior in behaviors.Reverse())
        {
            Func<Task<object?>> next = pipeline;

            pipeline = () => executionPlan.BehaviorInvoker(
                    behavior,
                    request,
                    next,
                    cancellationToken);
        }

        object? result = await pipeline().ConfigureAwait(false);

        if (result is null)
        {
            return default!;
        }

        return (TResponse)result;
    }
}
