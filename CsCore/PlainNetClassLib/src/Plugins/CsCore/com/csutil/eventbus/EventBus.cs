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

        public void Subscribe(object c, string key, Action a) { Add(c, key, a); }

        public void Subscribe<T>(object c, string key, Action<T> a) { Add(c, key, a); }
        public void Subscribe<T, V>(object c, string key, Action<T, V> a) { Add(c, key, a); }
        public void Subscribe<T, U, V>(object c, string key, Action<T, U, V> a) { Add(c, key, a); }

        public void Subscribe<T>(object c, string key, Func<T> f) { Add(c, key, f); }
        public void Subscribe<T, V>(object c, string key, Func<T, V> f) { Add(c, key, f); }
        public void Subscribe<T, U, V>(object c, string key, Func<T, U, V> f) { Add(c, key, f); }

        private void Add(object caller, string eventName, Delegate callback) {
            var noExistingListener = GetOrAdd(eventName).AddOrReplace(caller, callback);
            if (!noExistingListener) { Log.w("Existing listener was replaced for event=" + eventName); }
        }

        private ConcurrentDictionary<object, Delegate> GetOrAdd(string eventName) {
            return map.GetOrAdd(eventName, (_) => { return new ConcurrentDictionary<object, Delegate>(); });
        }

        public bool HasListenersFor(string eventName) { return map.ContainsKey(eventName); }

        public List<object> Publish(string eventName, params object[] args) {
            var results = new List<object>();
            ConcurrentDictionary<object, Delegate> dictForEventName;
            map.TryGetValue(eventName, out dictForEventName);
            if (!dictForEventName.IsNullOrEmpty()) {
                var listeners = dictForEventName.Values;
                foreach (var listener in listeners) {
                    try {
                        object result;
                        if (listener.DynamicInvokeV2(args, out result)) { results.Add(result); }
                    } catch (Exception e) { Log.e(e); }
                }
            } else {
                Log.w("No listeners registered for event: " + eventName);
            }
            return results;
        }

        public bool Unsubscribe(object caller, string eventName) {
            Delegate _;
            if (map[eventName].TryRemove(caller, out _)) {
                if (map[eventName].IsEmpty) return TryRemove(map, eventName);
            }
            return false;
        }

        public bool UnsubscribeAll(object caller) {

            var registeredEvents = map.Filter(x => x.Value.ContainsKey(caller));

            var callerRemovedEverywhere = true;
            foreach (var eventMaps in registeredEvents) {
                var eventName = eventMaps.Key;
                var listenersForEventName = eventMaps.Value;
                callerRemovedEverywhere &= TryRemove(listenersForEventName, caller);
                if (listenersForEventName.IsNullOrEmpty()) { TryRemove(map, eventName); }
            }
            return callerRemovedEverywhere;
        }

        private static bool TryRemove<K, V>(ConcurrentDictionary<K, V> self, K key) {
            V _; return self.TryRemove(key, out _);
        }

    }
}