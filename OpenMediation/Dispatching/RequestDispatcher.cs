using OpenMediation.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace OpenMediation.Dispatching;

internal sealed class RequestDispatcher(
    IServiceProvider serviceProvider,
    IRequestExecutionPlanCache executionPlanCache) : ISender
{
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        RequestExecutionPlan plan = executionPlanCache.GetOrAdd(request.GetType(), typeof(TResponse));

        // Resolve handler — exactly one required.
        object[] handlers = [.. serviceProvider.GetServices(plan.HandlerServiceType)];

        if (handlers.Length is not 1)
            ThrowHandlerCountException(request.GetType(), handlers.Length);

        // Resolve behaviors — empty array is the common case.
        object[] behaviors = [.. serviceProvider.GetServices(plan.BehaviorServiceType)];

        UntypedHandlerInvoker handlerInvoker = plan.HandlerInvoker;
        UntypedBehaviorInvoker behaviorInvoker = plan.BehaviorInvoker;
        object concreteRequest = request;
        object handler = handlers[0];

        // Innermost step: call the handler directly.
        Func<CancellationToken, Task<object?>> pipeline =
            ct => handlerInvoker(handler, concreteRequest, ct);

        // Wrap behaviors in reverse so first-registered is outermost.
        // Iterating backward avoids allocating a reversed copy.
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            object behavior = behaviors[i];
            Func<CancellationToken, Task<object?>> next = pipeline;
            pipeline = ct => behaviorInvoker(behavior, concreteRequest, next, ct);
        }

        object? result = await pipeline(cancellationToken).ConfigureAwait(false);
        return result is null ? default! : (TResponse)result;
    }

    private static void ThrowHandlerCountException(Type requestType, int count)
        => throw new InvalidOperationException(
            $"Expected exactly one handler for '{requestType.Name}' but found {count}. " +
            $"Ensure a single IRequestHandler<{requestType.Name}, TResponse> is registered.");
}