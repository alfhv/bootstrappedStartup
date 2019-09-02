# bootstrappedStartup

This repo contains severals samples as:

## Project webapiOverloadStartup.Bootstrap

Provide a way to use a Base StartupClass in a webapi where to put common functionalities. 
It is like inheritance but hidden and using the dotnet approach to call Startup methods, usually by convention.

BaseIStartup class is the Startup class dotnet will see and use in the pipeline.

Method UseBootstrapStartup() : call this method to wrap a client Startup class, it will look and work as a regular Startup.

## Project webapiOverloadStartup:

Implement the wrapped Startup class.

Also provide sample about how to use Unity as Service Provider. 
Also implement Interceptors (AOP) but without replacing the service provider.

### use Unity as Service Provider in the BaseStartup: 

To use Unity as container in a regular startup check : 
https://github.com/unitycontainer/microsoft-dependency-injection
Nuget package ```Unity.Microsoft.DependencyInjection```

The implementation I propose here is about how to do it with a BaseStartup 
(like having 2 startups and controlling the instanciation pipeline by yourself)

The container replacement is done, basically, implementing the method ConfigureContainer() in Statup class.
The problem is that I'm using a BaseStartup class and dotnetcore will look for ConfigureContainer there, and not in the child Startup.

in Program.cs call 

```
.AddUnityServiceProvider() 
```
This method call UseUnityServiceProvider() from ```Unity.Microsoft.DependencyInjection``` but also register a ```IStartoConfigureContainerFilter```
The filter implementatio is ```ConfigureStartupConfigureContainerFilter``` which will discover and call the ConfigureContainer() method on Startup class.

in Startup.cs implement 
```
void ConfigureContainer(IUnityContainer container) 
```

### Use Interceptors:

in Startup.cs check 
```
void ConfigureServices(IServiceCollection services) 
```

call method:

```
services.AddTransientForInterception<IDataService, DataService>(sc => sc.InterceptBy<TInterceptorData>());
```

Register IDataService interface, that is implemented by DataService class, and intercept this class with TInterceptorData class 

TInterceptorData implement interface Castle.DynamicProxy.IInterceptor which provide the method void 
```
Intercept(IInvocation invocation)
```
all we need to do the interception job.
