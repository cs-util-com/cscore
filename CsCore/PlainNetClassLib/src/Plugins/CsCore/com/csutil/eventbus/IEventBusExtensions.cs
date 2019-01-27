using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil {

    public static class IEventBusExtensions {

        public static void Subscribe(this IEventBus self, object s, string key, Action a) { self.Subscribe(s, key, a); }

        public static void Subscribe<T>(this IEventBus self, object s, string key, Action<T> a) { self.Subscribe(s, key, a); }
        public static void Subscribe<T, V>(this IEventBus self, object s, string key, Action<T, V> a) { self.Subscribe(s, key, a); }
        public static void Subscribe<T, U, V>(this IEventBus self, object s, string key, Action<T, U, V> a) { self.Subscribe(s, key, a); }

        public static void Subscribe<T>(this IEventBus self, object s, string key, Func<T> f) { self.Subscribe(s, key, f); }
        public static void Subscribe<T, V>(this IEventBus self, object s, string key, Func<T, V> f) { self.Subscribe(s, key, f); }
        public static void Subscribe<T, U, V>(this IEventBus self, object s, string key, Func<T, U, V> f) { self.Subscribe(s, key, f); }

        /// <summary> Uses the eventHistory of the eventbus to check if the event was already published at least once </summary>
        /// <returns> true if the callback was instantly invoked </returns>
        public static bool SubscribeForOnePublishOrInstantInvokeIfInHistory(this IEventBus self, string eventName, Action callback) {
            if (self.eventHistory.Contains(eventName)) {
                callback.InvokeIfNotNull();
                return true;
            } else {
                self.SubscribeForOnePublish(new object(), eventName, callback);
                return false;
            }
        }

        /// <summary> 
        /// After the first event was published this will directly unsubscribe again so that 
        /// only one event will ever be received by the callback 
        /// </summary>
        public static void SubscribeForOnePublish(this IEventBus self, object subscriber, string eventName, Action callback) {
            self.Subscribe(subscriber, eventName, () => {
                self.Unsubscribe(subscriber, eventName);
                callback.InvokeIfNotNull();
            });
        }
    }

}
