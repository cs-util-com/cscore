using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using com.csutil.eventbus;

namespace com.csutil {
    public class EventBus : IEventBus {

        static EventBus() { Log.d("EventBus used the first time.."); }
        public static IEventBus instance = new EventBus();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<object, Delegate>> map = new ConcurrentDictionary<string, ConcurrentDictionary<object, Delegate>>();

        public void Subscribe(object s, string key, Action a) { Add(s, key, a); }

        public void Subscribe<T>(object s, string key, Action<T> a) { Add(s, key, a); }
        public void Subscribe<T, V>(object s, string key, Action<T, V> a) { Add(s, key, a); }
        public void Subscribe<T, U, V>(object s, string key, Action<T, U, V> a) { Add(s, key, a); }

        public void Subscribe<T>(object s, string key, Func<T> f) { Add(s, key, f); }
        public void Subscribe<T, V>(object s, string key, Func<T, V> f) { Add(s, key, f); }
        public void Subscribe<T, U, V>(object s, string key, Func<T, U, V> f) { Add(s, key, f); }

        private void Add(object subscriber, string eventName, Delegate callback) {
            var replacedDelegate = GetOrAdd(eventName).AddOrReplace(subscriber, callback);
            if (replacedDelegate != null) { Log.w("Existing subscriber was replaced for event=" + eventName); }
        }

        private ConcurrentDictionary<object, Delegate> GetOrAdd(string eventName) {
            return map.GetOrAdd(eventName, (_) => { return new ConcurrentDictionary<object, Delegate>(); });
        }

        public ICollection<object> GetSubscribersFor(string eventName) {
            var subscribers = map.GetValue(eventName, null);
            return subscribers == null ? new List<object>() : subscribers.Keys;
        }

        public List<object> Publish(string eventName, params object[] args) {
            var results = new List<object>();
            ConcurrentDictionary<object, Delegate> dictForEventName;
            map.TryGetValue(eventName, out dictForEventName);
            if (!dictForEventName.IsNullOrEmpty()) {
                var subscribers = dictForEventName.Values;
                foreach (var subscriber in subscribers) {
                    try {
                        object result;
                        if (subscriber.DynamicInvokeV2(args, out result)) { results.Add(result); }
                    } catch (Exception e) { Log.e(e); }
                }
            } else {
                Log.d("No subscribers registered for event: " + eventName);
            }
            return results;
        }

        public bool Unsubscribe(object subscriber, string eventName) {
            if (!map.ContainsKey(eventName)) { return false; }
            Delegate _;
            if (map[eventName].TryRemove(subscriber, out _)) {
                if (map[eventName].IsEmpty) return TryRemove(map, eventName);
                return true;
            }
            return false;
        }

        public bool UnsubscribeAll(object subscriber) {
            var registeredEvents = map.Filter(x => x.Value.ContainsKey(subscriber));
            var removedCallbacks = new List<Delegate>();
            foreach (var eventMaps in registeredEvents) {
                var eventName = eventMaps.Key;
                var subscribersForEventName = eventMaps.Value;
                Delegate removedCallback;
                if (subscribersForEventName.TryRemove(subscriber, out removedCallback)) {
                    removedCallbacks.Add(removedCallback);
                }
                if (subscribersForEventName.IsNullOrEmpty()) { TryRemove(map, eventName); }
            }
            return removedCallbacks.Count > 0;
        }

        private static bool TryRemove<K, V>(ConcurrentDictionary<K, V> self, K key) {
            V _; return self.TryRemove(key, out _);
        }

    }
}