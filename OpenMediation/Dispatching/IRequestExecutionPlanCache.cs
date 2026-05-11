using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMediation.Dispatching;

/// <summary>
/// Internal contract for the request execution plan cache.
/// Keeping this internal prevents consumers from depending on the caching
/// strategy and allows the implementation to be swapped (e.g. for AOT).
/// </summary>
internal interface IRequestExecutionPlanCache
{
    RequestExecutionPlan GetOrAdd(Type requestType, Type responseType);
}

/// <summary>
/// Internal contract for the notification execution plan cache.
/// </summary>
internal interface INotificationExecutionPlanCache
{
    NotificationExecutionPlan GetOrAdd(Type notificationType);
}