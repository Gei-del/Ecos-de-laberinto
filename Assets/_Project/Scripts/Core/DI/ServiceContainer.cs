using System;
using System.Collections.Generic;

namespace EcosDelLaberinto.Core.DI
{
    /// <summary>
    /// Default <see cref="IServiceContainer"/>. Resolution order: an already created singleton
    /// wins; otherwise a registered factory is invoked once and its result cached as a singleton.
    /// </summary>
    public sealed class ServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, object> _singletons = new();
        private readonly Dictionary<Type, Func<IServiceContainer, object>> _factories = new();

        public void RegisterInstance<TService>(TService instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _singletons[typeof(TService)] = instance;
        }

        public void RegisterFactory<TService>(Func<IServiceContainer, TService> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _factories[typeof(TService)] = c => factory(c);
        }

        public TService Resolve<TService>()
        {
            if (TryResolve<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException(
                $"Service of type {typeof(TService).FullName} is not registered in the container.");
        }

        public bool TryResolve<TService>(out TService service)
        {
            var type = typeof(TService);

            if (_singletons.TryGetValue(type, out var existing))
            {
                service = (TService)existing;
                return true;
            }

            if (_factories.TryGetValue(type, out var factory))
            {
                var created = factory(this);
                _singletons[type] = created; // cache as singleton
                service = (TService)created;
                return true;
            }

            service = default;
            return false;
        }

        public bool IsRegistered<TService>()
        {
            var type = typeof(TService);
            return _singletons.ContainsKey(type) || _factories.ContainsKey(type);
        }
    }
}
