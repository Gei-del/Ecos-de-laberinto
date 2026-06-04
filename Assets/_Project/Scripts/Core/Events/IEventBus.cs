using System;

namespace EcosDelLaberinto.Core.Events
{
    /// <summary>
    /// Decoupled publish/subscribe bus (Observer pattern). Systems communicate through
    /// strongly typed events instead of holding direct references to each other, which is
    /// the backbone of the event-driven architecture.
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent;
        void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent;
        void Clear();
    }
}
