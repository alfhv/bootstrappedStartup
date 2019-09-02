using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace webapiOverloadStartup.Bootstrap
{
    /// <summary>
    /// To be implemented by bootstrated Startup classes to be sure .netcore startup-convention methods will be present
    /// </summary>
    public interface IBootstrapStartup
    {
        void Configure(IApplicationBuilder app, IHostingEnvironment env);
        void ConfigureServices(IServiceCollection services);
    }
}
