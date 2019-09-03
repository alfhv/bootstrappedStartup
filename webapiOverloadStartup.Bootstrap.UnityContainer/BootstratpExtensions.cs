using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace webapiOverloadStartup.Bootstrap.UnityContainer
{
    public static class BootstratpExtensions
    {
        public static IWebHostBuilder UseBootstrapStartupWithContainer<TStartup>(this IWebHostBuilder builder) where TStartup : class
        {
            builder
                .AddUnityServiceProvider()
                .UseStartup<BaseStartupWithContainer>()
                .ConfigureServices(services => // register delegate to call bootstraped class
                {
                    services.AddSingleton(typeof(IBootstrapStartup), typeof(TStartup));
                })
            ;

            return builder;
        }

        public static IWebHostBuilder AddUnityServiceProvider(this IWebHostBuilder builder)
        {
            builder
                .UseUnityServiceProvider(new Unity.UnityContainer())
                .ConfigureServices(
                s => s.AddSingleton<IStartupConfigureContainerFilter<IUnityContainer>>(
                    new ConfigureStartupConfigureContainerFilter()))
                ;

            return builder;
        }
    }
}
