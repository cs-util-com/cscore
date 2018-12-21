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
        public void Subscribe<T, V, U>(object c, string key, Action<T, V, U> a) { Add(c, key, a); }
        public void Subscribe<T, V, U>(object c, string key, Func<T> f) { Add(c, key, f); }
        public void Subscribe<T, V, U>(object c, string key, Func<T, V> f) { Add(c, key, f); }
        public void Subscribe<T, V, U>(object c, string key, Func<T, V, U> f) { Add(c, key, f); }

        private void Add(object caller, string eventName, Delegate callback) {
            var noExistingListener = GetOrAdd(eventName).AddOrReplace(caller, callback);
            if (!noExistingListener) {
                Log.w("Existing listener was replaced for event=" + eventName);
            }
        }

        private ConcurrentDictionary<object, Delegate> GetOrAdd(string eventName) {
            return map.GetOrAdd(eventName, (_) => { return new ConcurrentDictionary<object, Delegate>(); });
        }

        public bool HasListenersFor(string eventName) { return map.ContainsKey(eventName); }

        public List<object> Publish(string eventName, params object[] args) {
            var results = new List<object>();
            map.TryGetValue(eventName, out var dictForEventName);
            if (!dictForEventName.IsNullOrEmpty()) {
                var listeners = dictForEventName.Values;
                foreach (var listener in listeners) {
                    var p = listener.Method.GetParameters();
                    if (p.Length == args.Length) {
                        results.Add(listener.DynamicInvoke(args));
                    } else if (p.Length < args.Length) {
                        var subset = args.Take(p.Length).ToArray();
                        results.Add(listener.DynamicInvoke(subset));
                    } else {
                        Log.w("Listener skipped because not enough parameters passed: " + listener);
                    }
                }
            } else {
                Log.e("No listeners registered for event: " + eventName);
            }
            return results;
        }

        public bool Unsubscribe(object caller, string eventName) {
            if (map[eventName].TryRemove(caller, out Delegate _)) { if (map[eventName].IsEmpty) return UnsubscribeAll(eventName); }
            return false;
        }

        public bool UnsubscribeAll(string eventName) { return map.TryRemove(eventName, out ConcurrentDictionary<object, Delegate> _); }
    }
}