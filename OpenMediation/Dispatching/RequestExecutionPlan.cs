namespace OpenMediation.Dispatching;

/// <summary>
/// Invokes a typed IRequestHandler via a pre-compiled expression-tree lambda.
/// </summary>
internal delegate Task<object?> UntypedHandlerInvoker(
    object handler,
    object request,
    CancellationToken cancellationToken);

/// <summary>
/// Invokes a typed IPipelineBehavior via a pre-compiled expression-tree lambda.
/// next accepts CancellationToken so behaviors can substitute a modified token.
/// </summary>
internal delegate Task<object?> UntypedBehaviorInvoker(
    object behavior,
    object request,
    Func<CancellationToken, Task<object?>> next,
    CancellationToken cancellationToken);

/// <summary>
/// Invokes a typed INotificationHandler via a pre-compiled expression-tree lambda.
/// </summary>
internal delegate Task UntypedNotificationHandlerInvoker(
    object handler,
    object notification,
    CancellationToken cancellationToken);
/// <summary>
/// All pre-computed dispatch information for a given TRequest/TResponse pair.
/// Constructed once per unique type pair and stored in the concurrent cache.
/// </summary>
internal sealed class RequestExecutionPlan
{
    internal RequestExecutionPlan(
        Type handlerServiceType,
        Type behaviorServiceType,
        UntypedHandlerInvoker handlerInvoker,
        UntypedBehaviorInvoker behaviorInvoker)
    {
        HandlerServiceType = handlerServiceType ?? throw new ArgumentNullException(nameof(handlerServiceType));
        BehaviorServiceType = behaviorServiceType ?? throw new ArgumentNullException(nameof(behaviorServiceType));
        HandlerInvoker = handlerInvoker ?? throw new ArgumentNullException(nameof(handlerInvoker));
        BehaviorInvoker = behaviorInvoker ?? throw new ArgumentNullException(nameof(behaviorInvoker));
    }

    internal Type HandlerServiceType { get; }
    internal Type BehaviorServiceType { get; }
    internal UntypedHandlerInvoker HandlerInvoker { get; }
    internal UntypedBehaviorInvoker BehaviorInvoker { get; }
}
/// <summary>
/// All pre-computed dispatch information for a given notification type.
/// </summary>
internal sealed class NotificationExecutionPlan
{
    internal NotificationExecutionPlan(
        Type handlerServiceType,
        UntypedNotificationHandlerInvoker handlerInvoker)
    {
        HandlerServiceType = handlerServiceType ?? throw new ArgumentNullException(nameof(handlerServiceType));
        HandlerInvoker = handlerInvoker ?? throw new ArgumentNullException(nameof(handlerInvoker));
    }

    internal Type HandlerServiceType { get; }
    internal UntypedNotificationHandlerInvoker HandlerInvoker { get; }
}
