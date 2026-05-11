using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMediation.Dispatching;

public interface IRequestExecutionPlanCache
{
    RequestExecutionPlan GetOrAdd(Type requestType, Type responseType);
}
