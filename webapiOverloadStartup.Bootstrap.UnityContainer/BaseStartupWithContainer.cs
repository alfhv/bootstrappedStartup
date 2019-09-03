using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Unity;

namespace webapiOverloadStartup.Bootstrap.UnityContainer
{
    public class BaseStartupWithContainer : BaseStartup
    {
        public BaseStartupWithContainer(IConfiguration configuration, IBootstrapStartup bootstrapStartup) 
            : base(configuration, bootstrapStartup)
        {
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
