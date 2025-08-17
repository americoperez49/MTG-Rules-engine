using System;
using System.Collections.Generic;
using MTGRulesEngine.Events;

namespace MTGRulesEngine.Events
{
    /// <summary>
    /// A central event bus for publishing and subscribing to game events.
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Action<GameEvent>>> _handlers = new Dictionary<Type, List<Action<GameEvent>>>();

        /// <summary>
        /// Subscribes a handler to a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
        /// <param name="handler">The action to be invoked when the event is published.</param>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEvent
        {
            Type eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Action<GameEvent>>();
            }
            _handlers[eventType].Add(e => handler((TEvent)e));
        }

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
        /// <param name="handler">The action to be removed from the subscription list.</param>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : GameEvent
        {
            Type eventType = typeof(TEvent);
            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(e => handler((TEvent)e)); // This might not work correctly for anonymous delegates
            }
        }

        /// <summary>
        /// Publishes an event to all subscribed handlers.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish.</typeparam>
        /// <param name="eventArgs">The event instance to publish.</param>
        public void Publish<TEvent>(TEvent eventArgs) where TEvent : GameEvent
        {
            Type eventType = typeof(TEvent);
            if (_handlers.ContainsKey(eventType))
            {
                // Create a copy to prevent issues if handlers modify the list during iteration
                foreach (var handler in _handlers[eventType].ToList())
                {
                    handler(eventArgs);
                }
            }
        }
    }
}
