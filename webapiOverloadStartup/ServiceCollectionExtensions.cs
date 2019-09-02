using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace webapiOverloadStartup
{
    public static class ServiceCollectionExtensions
    {
        public static Dictionary<Type, Type> _interceptors = new Dictionary<Type, Type>();

        public static void AddTransientForInterception<TInterface, TImplementation>(this IServiceCollection services, 
            Action<IInterceptionRegistration> configureInterceptor)
            where TInterface : class where TImplementation : class, TInterface
        {
            // register implementation type so it can be resolved and have dependencies injected
            services.AddTransient<TImplementation>();
            
            var ci = new InterceptionRegistration();
            configureInterceptor.Invoke(ci);

            // register the interceptor so it can be resolved and have dependencies injected
            services.AddTransient(ci.InterceptorType);

            services.AddTransient<TInterface>(sp => 
            {
                var implementation = sp.GetRequiredService<TImplementation>();

                // resolve interceptor, dependencies, if any, will be injected
                var interceptor = (IInterceptor)sp.GetRequiredService(ci.InterceptorType);

                var proxyFactory = new ProxyGenerator();
                return proxyFactory.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
            });
        }
    }
}
