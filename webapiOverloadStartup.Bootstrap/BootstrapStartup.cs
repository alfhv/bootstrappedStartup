using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Unity.Microsoft.DependencyInjection;
using Unity;
using Microsoft.AspNetCore.Hosting.Internal;
using System.Reflection;

namespace webapiOverloadStartup.Bootstrap
{
    public class ConfigureStartupConfigureContainerFilter : IStartupConfigureContainerFilter<IUnityContainer>
    {       
        public ConfigureStartupConfigureContainerFilter()
        {
         
        }

        private static void CallStartupContainer(object startup, IUnityContainer container)
        {
            MethodInfo method = startup.GetType().GetMethod("ConfigureContainer");
            method.Invoke(startup, new object[] { container });
        }

        public Action<IUnityContainer> ConfigureContainer(Action<IUnityContainer> next)
        {
            return containerBuilder =>
            {
                next(containerBuilder);
                
                var startupClassInstance = containerBuilder.Resolve<IBootstrapStartup>();
                CallStartupContainer(startupClassInstance, containerBuilder);
            };
        }

    }

    public static class BootstratpExtensions
    {
        public static IWebHostBuilder UseBootstrapStartup<TStartup>(this IWebHostBuilder builder) where TStartup : class
        {
            //var startupType = typeof(TStartup);

            builder
                /*
                .ConfigureServices((context, services) =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var st = serviceProvider.GetService<IStartup>();

                    var uc = new UnityContainer();
                    uc.RegisterInstance(st);

                    //uc.RegisterSingleton( cbst);

                    var factory = new ServiceProviderFactory(uc);
                    services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IUnityContainer>>(factory));
                    services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>(factory));
                })
                */
                
                .UseStartup<BaseIStartup>()
                .ConfigureServices(services => // register delegate to call bootstraped class
                {
                    services.AddSingleton(typeof(IBootstrapStartup), typeof(TStartup));
                })
                
            ;

            return builder;
        }

        public static IWebHostBuilder AddUnityServiceProvider(this IWebHostBuilder builder)
        {
            builder.UseUnityServiceProvider(new UnityContainer())
                .ConfigureServices(
                s => s.AddSingleton<IStartupConfigureContainerFilter<IUnityContainer>>(
                    new ConfigureStartupConfigureContainerFilter()))
                ;

            return builder;
        }
    }

    

    public interface IBootstrapStartup
    {
        void Configure(IApplicationBuilder app, IHostingEnvironment env);
        void ConfigureServices(IServiceCollection services);
    }
                                    
    public class BaseIStartup //: IStartup
    {
        public IConfiguration Configuration { get; }

        public BaseIStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        IBootstrapStartup StartupClassInstance;

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var mvcBuilder = services.AddMvc();

            mvcBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerDocument();

            // Add extra global framework services

            // build provider to resolve injected bootstrapped class
            //var serviceProvider = services.BuildServiceProvider();

            StartupClassInstance = serviceProvider.GetService<IBootstrapStartup>();

            // call bootstrapped class method to register their own services 
            StartupClassInstance.ConfigureServices(services);

            // Load controllers in bootstrapped assembly
            var assembly = StartupClassInstance.GetType().Assembly;
            mvcBuilder.AddApplicationPart(assembly);

            // rebuild service provider to add bootstrapped registered services
            //return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi3();

            // call bootstraped method
            if (StartupClassInstance != null)
            {
                StartupClassInstance.Configure(app, env);
            }
        }

        public void ConfigureContainer(IUnityContainer container)
        {

            
            /*
            // Could be used to register more types
            container.RegisterType<IExternalService, ExternalService>(new Interceptor<InterfaceInterceptor>(),
                new InterceptionBehavior<LoggingAspect>());
                */
        }
    }

    
}
