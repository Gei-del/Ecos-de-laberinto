using System;

namespace EcosDelLaberinto.Core.DI
{
    /// <summary>
    /// Minimal, allocation-friendly dependency injection container. Supports singleton
    /// instances and lazy factories. Designed to be created once at boot and injected into
    /// every system, keeping classes testable and free of static singletons.
    /// </summary>
    public interface IServiceContainer
    {
        void RegisterInstance<TService>(TService instance);
        void RegisterFactory<TService>(Func<IServiceContainer, TService> factory);
        TService Resolve<TService>();
        bool TryResolve<TService>(out TService service);
        bool IsRegistered<TService>();
    }
}
