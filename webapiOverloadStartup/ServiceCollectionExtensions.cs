using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace webapiOverloadStartup
{
    public static class ServiceCollectionExtensions
    {
        public static Dictionary<Type, Type> _interceptors = new Dictionary<Type, Type>();

        public static void InterceptBy<TInterceptor>(this IServiceCollection serviceProvider)
            where TInterceptor : class, IInterceptor
        {
            serviceProvider.AddTransient<IInterceptor, TInterceptor>();
        }

        public static void AddTransientForInterception<TInterface, TImplementation>(this IServiceCollection services, 
            Action<IInterceptionRegistration> configureInterceptor)
            where TInterface : class where TImplementation : class, TInterface
        {
            services.AddTransient<TImplementation>();
            
            var ci = new InterceptionRegistration();
            configureInterceptor.Invoke(ci);
            services.AddTransient(ci.InterceptorType);

            services.AddTransient<TInterface>(sp => 
            {
                var implementation = sp.GetRequiredService<TImplementation>();

                var interceptor = (IInterceptor)sp.GetRequiredService(ci.InterceptorType);

                var proxyFactory = new ProxyGenerator();
                return proxyFactory.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
            });
        }
    }
}
