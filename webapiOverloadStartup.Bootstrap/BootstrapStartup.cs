using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace webapiOverloadStartup.Bootstrap
{
    public class BaseIStartup 
    {
        public IConfiguration Configuration { get; }

        public BaseIStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        IBootstrapStartup StartupClassInstance;

        public void ConfigureServices(IServiceCollection services)
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
