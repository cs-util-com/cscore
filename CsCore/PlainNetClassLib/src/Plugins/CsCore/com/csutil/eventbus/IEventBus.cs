using System;
using System.Collections.Generic;

namespace com.csutil.eventbus {
    public interface IEventBus {
        void Subscribe(object caller, string eventName, Action callback);
        void Subscribe<T>(object caller, string eventName, Action<T> callback);
        bool HasListenersFor(string eventName);
        void Subscribe<T, V>(object caller, string eventName, Action<T, V> callback);
        void Subscribe<T, V, U>(object caller, string eventName, Action<T, V, U> callback);
        void Subscribe<T, V, U>(object caller, string eventName, Func<T> callback);
        void Subscribe<T, V, U>(object caller, string eventName, Func<T, V> callback);
        void Subscribe<T, V, U>(object caller, string eventName, Func<T, V, U> callback);
        List<object> Publish(string eventName, params object[] parameters);
        bool Unsubscribe(object caller, string eventName);
        bool UnsubscribeAll(string eventName);
    }
}