using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unity;

namespace webapiOverloadStartup.Bootstrap
{
    public class BaseStartup 
    {
        private readonly IConfiguration Configuration;
        private readonly IBootstrapStartup StartupClassInstance;

        public BaseStartup(IConfiguration configuration, IBootstrapStartup bootstrapStartup)
        {
            Configuration = configuration;
            StartupClassInstance = bootstrapStartup;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddMvc();

            mvcBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerDocument();

            // Add extra global framework services

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

        
    }  
}
