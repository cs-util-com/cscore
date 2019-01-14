using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.csutil.eventbus {
    public interface IEventBus {

        List<object> Publish(string eventName, params object[] parameters);

        void Subscribe(object subscriber, string eventName, Action callback);
        void Subscribe<T>(object subscriber, string eventName, Action<T> callback);
        void Subscribe<T, V>(object subscriber, string eventName, Action<T, V> callback);
        void Subscribe<T, U, V>(object subscriber, string eventName, Action<T, U, V> callback);
        void Subscribe<T>(object subscriber, string eventName, Func<T> callback);
        void Subscribe<T, V>(object subscriber, string eventName, Func<T, V> callback);
        void Subscribe<T, U, V>(object subscriber, string eventName, Func<T, U, V> callback);

        ICollection<object> GetSubscribersFor(string eventName);

        bool Unsubscribe(object subscriber, string eventName);
        bool UnsubscribeAll(object subscriber);

        /// <summary> A list of events that where already published by the event bus </summary>
        ConcurrentQueue<string> eventHistory { get; set; }

    }
}