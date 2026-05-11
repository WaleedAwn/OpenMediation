using OpenMediation.Abstractions;
using OpenMediation.Pipeline;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenMediation.Dispatching;

// NOTE ON NATIVE AOT:
// Expression.Compile() uses Reflection.Emit and is NOT compatible with Native AOT.
// The cache sits behind IRequestExecutionPlanCache so a source-generated AOT-safe
// implementation can be swapped in at registration time without any public API change.

internal sealed class RequestExecutionPlanCache : IRequestExecutionPlanCache
{
    // Static so the cache survives ServiceProvider rebuilds in test scenarios.
    private static readonly ConcurrentDictionary<ExecutionPlanCacheKey, RequestExecutionPlan> s_cache = new();

    public RequestExecutionPlan GetOrAdd(Type requestType, Type responseType)
    {
        var key = new ExecutionPlanCacheKey(requestType, responseType);
        return s_cache.GetOrAdd(key, static k => BuildExecutionPlan(k.RequestType, k.ResponseType));
    }

    private static RequestExecutionPlan BuildExecutionPlan(Type requestType, Type responseType)
    {
        Type handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        Type behaviorServiceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

        return new RequestExecutionPlan(
            handlerServiceType,
            behaviorServiceType,
            BuildHandlerInvoker(requestType, responseType, handlerServiceType),
            BuildBehaviorInvoker(requestType, responseType, behaviorServiceType));
    }

    // AOT: Replace with a source-generated switch/dictionary over known TRequest types.
    private static UntypedHandlerInvoker BuildHandlerInvoker(
        Type requestType, Type responseType, Type handlerServiceType)
    {
        MethodInfo handleMethod = handlerServiceType.GetMethod(
            "Handle", [requestType, typeof(CancellationToken)])!;

        ParameterExpression handlerParam = Expression.Parameter(typeof(object), "handler");
        ParameterExpression requestParam = Expression.Parameter(typeof(object), "request");
        ParameterExpression tokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        MethodInfo boxMethod = typeof(RequestExecutionPlanCache)
            .GetMethod(nameof(BoxTaskResultAsync), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(responseType);

        Expression body = Expression.Call(
            boxMethod,
            Expression.Call(
                Expression.Convert(handlerParam, handlerServiceType),
                handleMethod,
                Expression.Convert(requestParam, requestType),
                tokenParam));

        return Expression.Lambda<UntypedHandlerInvoker>(body, handlerParam, requestParam, tokenParam)
                         .Compile();
    }

    // AOT: Replace with a source-generated switch/dictionary over known TRequest types.
    private static UntypedBehaviorInvoker BuildBehaviorInvoker(
        Type requestType, Type responseType, Type behaviorServiceType)
    {
        Type nextDelegateType = typeof(RequestHandlerDelegate<>).MakeGenericType(responseType);

        MethodInfo handleMethod = behaviorServiceType.GetMethod(
            "Handle", [requestType, nextDelegateType, typeof(CancellationToken)])!;

        ParameterExpression behaviorParam = Expression.Parameter(typeof(object), "behavior");
        ParameterExpression requestParam = Expression.Parameter(typeof(object), "request");
        ParameterExpression nextParam = Expression.Parameter(typeof(Func<CancellationToken, Task<object?>>), "next");
        ParameterExpression tokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        MethodInfo adaptNextMethod = typeof(RequestExecutionPlanCache)
            .GetMethod(nameof(AdaptNext), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(responseType);

        MethodInfo boxMethod = typeof(RequestExecutionPlanCache)
            .GetMethod(nameof(BoxTaskResultAsync), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(responseType);

        Expression body = Expression.Call(
            boxMethod,
            Expression.Call(
                Expression.Convert(behaviorParam, behaviorServiceType),
                handleMethod,
                Expression.Convert(requestParam, requestType),
                Expression.Call(adaptNextMethod, nextParam),
                tokenParam));

        return Expression.Lambda<UntypedBehaviorInvoker>(body, behaviorParam, requestParam, nextParam, tokenParam)
                         .Compile();
    }

    public static async Task<object?> BoxTaskResultAsync<TResponse>(Task<TResponse> task)
        => await task.ConfigureAwait(false);

    public static RequestHandlerDelegate<TResponse> AdaptNext<TResponse>(
        Func<CancellationToken, Task<object?>> next)
        => async ct =>
        {
            object? result = await next(ct).ConfigureAwait(false);
            return result is null ? default! : (TResponse)result;
        };

    private readonly record struct ExecutionPlanCacheKey(Type RequestType, Type ResponseType);
}