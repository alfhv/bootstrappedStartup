using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using webapiOverloadStartup.Bootstrap;
using Unity.Microsoft.DependencyInjection;
using Unity;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace webapiOverloadStartup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)               
                .UseBootstrapStartupWithContainer<MinimalStartup>()
                ;
    }
}
