using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace webapiOverloadStartup.Bootstrap
{
    // All pipeline is based on .net Core StartupLoader:
    // https://raw.githubusercontent.com/aspnet/Hosting/f9d145887773e0c650e66165e0c61886153bcc0b/src/Microsoft.AspNetCore.Hosting/Internal/StartupLoader.cs
    // StartupFilter1
    //   StartupFilter2
    //     ConfigureServices
    //   StartupFilter2
    // StartupFilter1
    // ConfigureContainerFilter1
    //   ConfigureContainerFilter2
    //     ConfigureContainer
    //   ConfigureContainerFilter2
    // ConfigureContainerFilter1

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
            builder
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

        /// <summary>
        /// dont use this method, just to be sure it is called
        /// </summary>
        /// <param name="container"></param>
        public void ConfigureContainer(IUnityContainer container)
        {
            // do nothing here as registration should be done in bootstrated class
        }
    }  
}
