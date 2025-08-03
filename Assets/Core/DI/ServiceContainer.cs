using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ServiceContainer
{
    private static readonly Lazy<ServiceContainer> _instance =
        new(() => new ServiceContainer());

    public static ServiceContainer Instance => _instance.Value;
    private readonly Dictionary<Type, ServiceDescriptor> _descriptors = new();
    private ServiceContainer() { }

    public void RegisterAsSingleton<TInterface>(Func<TInterface> factory) where TInterface : class
    {
        Register(factory, Lifetime.Singleton);
    }
    public void RegisterAsScoped<TInterface>(Func<TInterface> factory) where TInterface : class
    {
        Register(factory, Lifetime.Scoped);
    }
    public void RegisterAsTransient<TInterface>(Func<TInterface> factory) where TInterface : class
    {
        Register(factory, Lifetime.Transient);
    }
    private void Register<TInterface>(Func<TInterface> factory, Lifetime lifetime) where TInterface : class
    {
        var interfaceType = typeof(TInterface);
        if (!interfaceType.IsInterface)
            throw new InvalidOperationException($"{interfaceType.Name} must be an interface.");
        if (_descriptors.ContainsKey(interfaceType))
            throw new InvalidOperationException($"{interfaceType.Name} is already registered.");

        _descriptors[interfaceType] = new ServiceDescriptor(factory, lifetime);
    }

    public void RegisterAsSingleton<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
    {
        Register<TInterface, TImplementation>(Lifetime.Singleton);
    }

    public void RegisterAsScoped<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
    {
        Register<TInterface, TImplementation>(Lifetime.Scoped);
    }

    public void RegisterAsTransient<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        Register<TInterface, TImplementation>(Lifetime.Transient);
    }

    private void Register<TInterface, TImplementation>(Lifetime lifetime)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var interfaceType = typeof(TInterface);
        var implementationType = typeof(TImplementation);

        if (!interfaceType.IsInterface)
            throw new InvalidOperationException($"{interfaceType.Name} must be an interface.");
        if (_descriptors.ContainsKey(interfaceType))
            throw new InvalidOperationException($"{interfaceType.Name} is already registered.");

        _descriptors[interfaceType] = new ServiceDescriptor(
            () => CreateWithConstructorInjection(implementationType, new HashSet<Type>()),
            lifetime
        );
    }

    public TInterface Resolve<TInterface>()
    {
        return (TInterface)Resolve(typeof(TInterface), new HashSet<Type>());
    }

    private object Resolve(Type type, HashSet<Type> resolving)
    {
        if (!_descriptors.TryGetValue(type, out var descriptor))
            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");

        if (resolving.Contains(type))
            throw new InvalidOperationException($"Circular dependency detected while resolving {type.Name}");

        resolving.Add(type);

        switch (descriptor.Lifetime)
        {
            case Lifetime.Singleton:
                if (descriptor.Instance == null)
                    descriptor.Instance = descriptor.Factory();
                resolving.Remove(type);
                return descriptor.Instance;

            case Lifetime.Scoped:
                if (descriptor.Instance == null)
                    descriptor.Instance = descriptor.Factory();
                resolving.Remove(type);
                return descriptor.Instance;

            case Lifetime.Transient:
                var transient = descriptor.Factory();
                resolving.Remove(type);
                return transient;

            default:
                throw new NotSupportedException($"Unsupported lifetime: {descriptor.Lifetime}");
        }
    }

    private object CreateWithConstructorInjection(Type implementationType, HashSet<Type> resolving)
    {
        var constructors = implementationType.GetConstructors();

        if (constructors.Length == 0)
            throw new InvalidOperationException($"No public constructor found for {implementationType.Name}");

        if (constructors.Length > 1)
            throw new InvalidOperationException($"Multiple constructors found for {implementationType.Name}");

        var constructor = constructors.First();

        var parameters = constructor.GetParameters();
        var resolvedParameters = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            resolvedParameters[i] = Resolve(paramType, resolving);
        }

        return constructor.Invoke(resolvedParameters);
    }

    public bool IsRegistered<TInterface>() => _descriptors.ContainsKey(typeof(TInterface));
    public void Clear() => _descriptors.Clear();
    public void ResetScopedInstances()
    {
        var scopedDescriptors = _descriptors.Where(desc => desc.Value.Lifetime == Lifetime.Scoped);
        foreach (var item in scopedDescriptors)
        {
            item.Value.Instance = null;
        }
    }
}