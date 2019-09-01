using Castle.DynamicProxy;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using webapiOverloadStartup.Bootstrap;

namespace webapiOverloadStartup
{
    public class MinimalStartup : IBootstrapStartup
    {
        public MinimalStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDependencyForExternalService, DependencyForExternalService>();
            //services.AddScoped<IExternalService, ExternalService>();
            services.AddTransientForInterception<IDataService, DataService>(sc => sc.InterceptBy<TInterceptorData>());
            //services.AddTransientForInterception<IExternalService, ExternalService>(sc => sc.InterceptBy<TInterceptorExternal>());

            //services.AddScoped<IDependencyForExternalService, DependencyForExternalService>();

            //ConfigureContainer(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            AddFilter((DataService m) => m.GetValues(), m => m.Where(s => s.Contains("1")).ToList());
        }

        public void ConfigureContainer(IUnityContainer container)
        {
            container.AddNewExtension<Interception>();
            // Could be used to register more types
            container.RegisterType<IExternalService, ExternalService>(new Interceptor<InterfaceInterceptor>(), 
                new InterceptionBehavior<LoggingAspect>());
        }

        private void AddFilter<T, TResult>(Expression<Func<T, TResult>> targetMethod, Func<TResult, TResult> filter)
        {
            MethodFilters.Add(new MethodFilter { TargetMethod = targetMethod, Filter = filter });
        }

        public static List<MethodFilter> MethodFilters = new List<MethodFilter>();
    }

    public class MethodFilter
    {
        public Expression TargetMethod { get; internal set; }
        public Delegate Filter { get; internal set; }
    }

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

    public class TInterceptorData : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            var a = MinimalStartup.MethodFilters[0].Filter.DynamicInvoke(invocation.ReturnValue);
        }
    }

    public class TInterceptorExternal : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerDocument();

            services.AddScoped<IDataService, DataService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi3();
            
        }
    }
}
