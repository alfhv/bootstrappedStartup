using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapiOverloadStartup
{
    public interface IInterceptionRegistration
    {
        void InterceptBy<TInterceptor>() where TInterceptor : class, IInterceptor;
    }

    public class InterceptionRegistration : IInterceptionRegistration
    {
        //public TImp _Implementation;
        public Type InterceptorType;

        public InterceptionRegistration()
        {
        }

        public void InterceptBy<TInterceptor>()
            where TInterceptor : class, IInterceptor
        {
            InterceptorType = typeof(TInterceptor);
        }
    }

    public class TInterceptorData : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            var a = MinimalStartup.MethodFilters[0].Filter.DynamicInvoke(invocation.ReturnValue); // just checking
        }
    }

    public class TInterceptorExternal : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            throw new System.NotImplementedException(); // do nothing for now, just be sure is called
        }
    }
}
