using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace webapiOverloadStartup.Bootstrap
{
    public static class BootstratpExtensions
    {
        public static IWebHostBuilder UseBootstrapStartup<TStartup>(this IWebHostBuilder builder) where TStartup : class
        {
            builder
                .UseStartup<TStartup>()
                .ConfigureServices(services => // register delegate to call bootstraped class
                {
                    services.AddSingleton(typeof(IBootstrapStartup), typeof(TStartup));
                    services.AddSingleton<BaseStartup>();
                    services.AddSingleton<IStartupConfigureServicesFilter, BaseConfigureServicesFilter>();
                    services.AddSingleton<IStartupFilter, BaseStartupFilter>();
                })
            ;

            return builder;
        }
    }

    internal class BaseConfigureServicesFilter : IStartupConfigureServicesFilter
    {
        BaseStartup _baseStartup;
        public BaseConfigureServicesFilter(BaseStartup baseStartup)
        {
            _baseStartup = baseStartup;
        }


        public Action<IServiceCollection> ConfigureServices(Action<IServiceCollection> next)
        {
            return sc =>
            {
                _baseStartup.ConfigureServices(sc);

                next(sc);
            };
        }
    }

    internal class BaseStartupFilter : IStartupFilter
    {
        BaseStartup _baseStartup;
        public BaseStartupFilter(BaseStartup baseStartup)
        {
            _baseStartup = baseStartup;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _baseStartup.Configure(app);

                next(app);

                app.UseMvc();
            };
        }
    }
}
