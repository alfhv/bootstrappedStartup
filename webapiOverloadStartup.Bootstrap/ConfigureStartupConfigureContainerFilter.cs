using Microsoft.AspNetCore.Hosting.Internal;
using System;
using System.Reflection;
using Unity;

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

        // simple reflection to find and call ConfigureContainer() on bootstrapped class
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
}
