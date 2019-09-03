using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.PolicyInjection.Pipeline;

namespace webapiOverloadStartup
{
    public class LoggingAspect : IInterceptionBehavior
    {
        public bool WillExecute => true;

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            // Invoke the next behavior in the chain. 
            var result = getNext()(input, getNext);

            return result;
        }
    }
}
