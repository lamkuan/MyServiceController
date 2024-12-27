namespace MyServiceController.Container;

public class SimpleIoCContainer
{
    private readonly Dictionary<Type, ServiceDescriptor> _services = new();
    
    // 注冊服務
    public void AddTransient<TService, TImplementation>()
        where TService : class 
        where TImplementation : class, TService
    {
        var lifetime = ServiceLifetime.Transient;
        _services[typeof(TService)] = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
    }

    public void AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        var lifetime = ServiceLifetime.Singleton;
        _services[typeof(TService)] = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
    }

    public TService Resolve<TService>() where TService : class
    {
        var instance = Resolve(typeof(TService));

        switch (instance)
        {
            case null:
                throw new Exception($"Could not resolve service {typeof(TService).Name}");
            case TService service:
                return service;
        }
        
        throw new Exception($"Could not resolve service {typeof(TService).Name}");
    }
    
    // 解釋服務
    private object? Resolve(Type serviceType)
    {
        if (!_services.TryGetValue(serviceType, out var descriptor))
        {
            throw new InvalidOperationException($"Service type {serviceType} is not registered");
        }

        switch (descriptor.Lifetime)
        {
            // Singleton 模式
            case ServiceLifetime.Singleton:
                // 如果ImplementationInstance已經被實現，則直接返回實例。
                if (descriptor.ImplementationInstance != null)
                {
                    return descriptor.ImplementationInstance;
                }
                
                var singletonConstructor = descriptor.ImplementationType.GetConstructors().FirstOrDefault();
                var singletonParameters = singletonConstructor?.GetParameters();
                
                // 遞歸解析構造函數參數
                var singletonParameterInstances = singletonParameters?.Select(p => Resolve(p.ParameterType)).ToArray();
                
                var singletonInstance = Activator.CreateInstance(descriptor.ImplementationType, singletonParameterInstances);
                
                // 緩存實例
                descriptor.ImplementationInstance = singletonInstance;

                return singletonInstance;
            
            // Transient 模式
            case ServiceLifetime.Transient:
                var transientConstructor = descriptor.ImplementationType.GetConstructors().FirstOrDefault();
                var transientParameters = transientConstructor?.GetParameters();
                
                // 遞歸解析構造函數參數
                var transientParameterInstances = transientParameters?.Select(p => Resolve(p.ParameterType)).ToArray();
                
                // 創建實例
                var transientInstance = Activator.CreateInstance(descriptor.ImplementationType, transientParameterInstances);

                return transientInstance;
        }

        return null;
    }
}