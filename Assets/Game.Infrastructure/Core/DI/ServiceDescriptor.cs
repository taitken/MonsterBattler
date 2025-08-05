using System;

public class ServiceDescriptor
{
    public Func<object> Factory { get; }
    public Lifetime Lifetime { get; }
    public object Instance { get; set; }  // Used for Singleton

    public ServiceDescriptor(Func<object> factory, Lifetime lifetime)
    {
        Factory = factory;
        Lifetime = lifetime;
    }
}