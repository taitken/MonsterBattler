using System;

namespace Game.Core
{
    public interface IServiceContainer
    {
        void RegisterAsSingleton<TInterface>(Func<TInterface> factory) where TInterface : class;
        void RegisterAsScoped<TInterface>(Func<TInterface> factory) where TInterface : class;
        void RegisterAsTransient<TInterface>(Func<TInterface> factory) where TInterface : class;

        void RegisterAsSingleton<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        void RegisterAsScoped<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        void RegisterAsTransient<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        TInterface Resolve<TInterface>();
        bool IsRegistered<TInterface>();
        void Clear();
        void ResetScopedInstances();
    }
}
