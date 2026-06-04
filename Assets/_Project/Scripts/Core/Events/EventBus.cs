using System;
using System.Collections.Generic;

namespace EcosDelLaberinto.Core.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/> implementation. Handlers are stored per event type.
    /// Publishing iterates over a snapshot so subscribers may unsubscribe while being notified
    /// without invalidating the enumeration. Exceptions thrown by a handler are isolated so a
    /// single faulty listener cannot break the whole dispatch.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }

            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            if (_handlers.TryGetValue(typeof(TEvent), out var list))
            {
                list.Remove(handler);
            }
        }

        public void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list) || list.Count == 0)
            {
                return;
            }

            // Snapshot to tolerate (un)subscription during dispatch.
            var snapshot = list.ToArray();
            foreach (var del in snapshot)
            {
                if (del is Action<TEvent> typed)
                {
                    try
                    {
                        typed.Invoke(gameEvent);
                    }
                    catch (Exception ex)
                    {
                        Utils.GameLogger.Error($"EventBus handler for {typeof(TEvent).Name} threw: {ex}");
                    }
                }
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
