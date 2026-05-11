using OpenMediation.Abstractions;
using OpenMediation.Pipeline;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenMediation.Dispatching;

public sealed class RequestExecutionPlanCache : IRequestExecutionPlanCache
{
    private static readonly ConcurrentDictionary<ExecutionPlanCacheKey, RequestExecutionPlan> Cache = new();

    public RequestExecutionPlan GetOrAdd(Type requestType, Type responseType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        ArgumentNullException.ThrowIfNull(responseType);

        var key = new ExecutionPlanCacheKey(requestType, responseType);

        return Cache.GetOrAdd(key, static key => BuildExecutionPlan(key.RequestType, key.ResponseType));
    }

    private static RequestExecutionPlan BuildExecutionPlan(Type requestType, Type responseType)
    {
        Type handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        Type behaviorServiceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

        UntypedHandlerInvoker handlerInvoker = BuildHandlerInvoker(requestType, responseType, handlerServiceType);
        UntypedBehaviorInvoker behaviorInvoker = BuildBehaviorInvoker(requestType, responseType, behaviorServiceType);

        return new RequestExecutionPlan(
            handlerServiceType,
            behaviorServiceType,
            handlerInvoker,
            behaviorInvoker);
    }

    private static UntypedHandlerInvoker BuildHandlerInvoker(Type requestType, Type responseType, Type handlerServiceType)
    {
        MethodInfo handleMethod = handlerServiceType.GetMethod("Handle", [requestType, typeof(CancellationToken)])
            ?? throw new InvalidOperationException("Could not find Handle method.");

        ParameterExpression handlerParameter = Expression.Parameter(typeof(object), "handler");
        ParameterExpression requestParameter = Expression.Parameter(typeof(object), "request");
        ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        UnaryExpression castedHandler = Expression.Convert(handlerParameter, handlerServiceType);
        UnaryExpression castedRequest = Expression.Convert(requestParameter, requestType);

        MethodCallExpression handleCall = Expression.Call(castedHandler, handleMethod, castedRequest, cancellationTokenParameter);

        MethodInfo boxTaskResultMethod = typeof(RequestExecutionPlanCache)
                .GetMethod(nameof(BoxTaskResultAsync), BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(responseType);

        return Expression.Lambda<UntypedHandlerInvoker>(Expression.Call(boxTaskResultMethod, handleCall),
            handlerParameter, requestParameter, cancellationTokenParameter).Compile();
    }

    private static UntypedBehaviorInvoker BuildBehaviorInvoker(Type requestType, Type responseType, Type behaviorServiceType)
    {
        Type requestHandlerDelegateType = typeof(RequestHandlerDelegate<>).MakeGenericType(responseType);
        MethodInfo handleMethod = behaviorServiceType.GetMethod("Handle", [requestType, requestHandlerDelegateType, typeof(CancellationToken)])
            ?? throw new InvalidOperationException("Could not find Handle method.");

        ParameterExpression behaviorParameter = Expression.Parameter(typeof(object), "behavior");
        ParameterExpression requestParameter = Expression.Parameter(typeof(object), "request");
        ParameterExpression nextParameter = Expression.Parameter(typeof(Func<Task<object?>>), "next");
        ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        MethodInfo adaptNextMethod = typeof(RequestExecutionPlanCache).GetMethod(nameof(AdaptNext), BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(responseType);
        MethodCallExpression typedNextDelegate = Expression.Call(adaptNextMethod, nextParameter);

        MethodCallExpression handleCall = Expression.Call(Expression.Convert(behaviorParameter, behaviorServiceType),
            handleMethod, Expression.Convert(requestParameter, requestType), typedNextDelegate, cancellationTokenParameter);

        MethodInfo boxTaskResultMethod = typeof(RequestExecutionPlanCache).GetMethod(nameof(BoxTaskResultAsync), BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(responseType);

        return Expression.Lambda<UntypedBehaviorInvoker>(Expression.Call(boxTaskResultMethod, handleCall),
            behaviorParameter, requestParameter, nextParameter, cancellationTokenParameter).Compile();
    }

    public static async Task<object?> BoxTaskResultAsync<TResponse>(Task<TResponse> task) => await task.ConfigureAwait(false);

    public static RequestHandlerDelegate<TResponse> AdaptNext<TResponse>(Func<Task<object?>> next)
    {
        return async () =>
        {
            object? result = await next().ConfigureAwait(false);
            return result is null ? default! : (TResponse)result;
        };
    }

    private readonly record struct ExecutionPlanCacheKey(Type RequestType, Type ResponseType);
}