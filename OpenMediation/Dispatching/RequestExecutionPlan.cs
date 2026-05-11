namespace OpenMediation.Dispatching;

/// <summary>
/// Represents the cached execution metadata for a request/response pair.
/// </summary>
public sealed class RequestExecutionPlan(
    Type handlerServiceType,
    Type behaviorServiceType,
    UntypedHandlerInvoker handlerInvoker,
    UntypedBehaviorInvoker behaviorInvoker)
{
    public Type HandlerServiceType { get; } = handlerServiceType ?? throw new ArgumentNullException(nameof(handlerServiceType));
    public Type BehaviorServiceType { get; } = behaviorServiceType ?? throw new ArgumentNullException(nameof(behaviorServiceType));
    public UntypedHandlerInvoker HandlerInvoker { get; } = handlerInvoker ?? throw new ArgumentNullException(nameof(handlerInvoker));
    public UntypedBehaviorInvoker BehaviorInvoker { get; } = behaviorInvoker ?? throw new ArgumentNullException(nameof(behaviorInvoker));
}
public delegate Task<object?> UntypedHandlerInvoker(
    object handler,
    object request,
    CancellationToken cancellationToken);

public delegate Task<object?> UntypedBehaviorInvoker(
    object behavior,
    object request,
    Func<Task<object?>> next,
    CancellationToken cancellationToken);
