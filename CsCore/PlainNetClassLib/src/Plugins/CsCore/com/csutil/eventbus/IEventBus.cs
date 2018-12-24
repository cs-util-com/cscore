using System;
using System.Collections.Generic;

namespace com.csutil.eventbus {
    public interface IEventBus {
        void Subscribe(object caller, string eventName, Action callback);
        void Subscribe<T>(object caller, string eventName, Action<T> callback);
        void Subscribe<T, V>(object caller, string eventName, Action<T, V> callback);
        void Subscribe<T, U, V>(object caller, string eventName, Action<T, U, V> callback);
        void Subscribe<T>(object caller, string eventName, Func<T> callback);
        void Subscribe<T, V>(object caller, string eventName, Func<T, V> callback);
        void Subscribe<T, U, V>(object caller, string eventName, Func<T, U, V> callback);
        List<object> Publish(string eventName, params object[] parameters);
        bool HasListenersFor(string eventName);
        bool Unsubscribe(object caller, string eventName);
        bool UnsubscribeAll(object caller);
    }
}