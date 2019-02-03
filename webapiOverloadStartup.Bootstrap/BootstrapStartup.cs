using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;

namespace webapiOverloadStartup.Bootstrap
{
    public static class BootstratpExtensions
    {
        public static IWebHostBuilder UseBootstrapStartup<TStartup>(this IWebHostBuilder builder) where TStartup : class
        {
            var startupType = typeof(TStartup);

            builder
                .UseStartup<BaseIStartup>()
                .ConfigureServices(services => // register delegate to call bootstraped class
                {
                    services.AddSingleton(typeof(IBootstrapStartup), typeof(TStartup));
                })
                ;

            return builder;
        }
    }

    public interface IBootstrapStartup
    {
        void Configure(IApplicationBuilder app, IHostingEnvironment env);
        void ConfigureServices(IServiceCollection services);
    }
                                    
    public class BaseIStartup : IStartup
    {
        public IConfiguration Configuration { get; }

        public BaseIStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        IBootstrapStartup StartupClassInstance;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddMvc();
            mvcBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerDocument();

            // Add extra global framework services

            // build provider to resolve injected bootstrapped class
            var serviceProvider = services.BuildServiceProvider();

            StartupClassInstance = serviceProvider.GetService<IBootstrapStartup>();

            // call bootstrapped class method to register their own services 
            StartupClassInstance.ConfigureServices(services);

            // Load controllers in bootstrapped assembly
            var assembly = StartupClassInstance.GetType().Assembly;
            mvcBuilder.AddApplicationPart(assembly);

            // rebuild service provider to add bootstrapped registered services
            return services.BuildServiceProvider(); ;
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
    }

    public class BaseStartup
    {
        public BaseStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerDocument();

            //services.BuildServiceProvider().GetService<IBootstratStartup>().ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(ApplicationBuilder app, IHostingEnvironment env)
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
