using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        public IConfiguration Configuration { get; }

        public MinimalStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDependencyForExternalService, DependencyForExternalService>();
            //services.AddScoped<IExternalService, ExternalService>();
            services.AddTransientForInterception<IDataService, DataService>(sc => sc.InterceptBy<TInterceptorData>());
            //services.AddTransientForInterception<IExternalService, ExternalService>(sc => sc.InterceptBy<TInterceptorExternal>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            AddFilter((DataService m) => m.GetValues(), m => m.Where(s => s.Contains("1")).ToList());
        }

        public void ConfigureContainer(IUnityContainer container)
        {
            // Register your types in UnityContainer

            container.AddNewExtension<Interception>();
            
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
}
