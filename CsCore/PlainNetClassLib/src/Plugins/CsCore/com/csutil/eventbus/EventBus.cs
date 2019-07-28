using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using com.csutil.eventbus;

namespace com.csutil {

    public class EventBus : IEventBus {

        static EventBus() {
            // Log.d("EventBus used the first time..");
        }

        public static IEventBus instance = new EventBus();

        public ConcurrentQueue<string> eventHistory { get; set; }

        /// <summary> sync subscribing and publishing to not happen at the same time </summary>
        private object threadLock = new object();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<object, Delegate>> map =
            new ConcurrentDictionary<string, ConcurrentDictionary<object, Delegate>>();

        public EventBus() {
            eventHistory = new ConcurrentQueue<string>();
        }

        public void Subscribe(object subscriber, string eventName, Delegate callback) {
            lock (threadLock) {
                var replacedDelegate = GetOrAdd(eventName).AddOrReplace(subscriber, callback);
                if (replacedDelegate != null) { Log.w("Existing subscriber was replaced for event=" + eventName); }
            }
        }

        private ConcurrentDictionary<object, Delegate> GetOrAdd(string eventName) {
            return map.GetOrAdd(eventName, (_) => { return new ConcurrentDictionary<object, Delegate>(); });
        }

        public ICollection<object> GetSubscribersFor(string eventName) {
            var subscribers = map.GetValue(eventName, null);
            return subscribers == null ? new List<object>() : subscribers.Keys;
        }

        public List<object> Publish(string eventName, params object[] args) {
            return NewPublishIEnumerable(eventName, args).ToList();
        }

        public IEnumerable<object> NewPublishIEnumerable(string eventName, params object[] args) {
            lock (threadLock) {
                eventHistory.Enqueue(eventName);
                ConcurrentDictionary<object, Delegate> dictForEventName;
                map.TryGetValue(eventName, out dictForEventName);
                if (!dictForEventName.IsNullOrEmpty()) {
                    var subscribers = dictForEventName.Values;
                    return subscribers.Map(subscriber => {
                        try {
                            object result;
                            if (subscriber.DynamicInvokeV2(args, out result)) { return result; }
                        } catch (Exception e) { Log.e(e); }
                        return null;
                    });
                }
                return new List<object>();
            }
        }

        public bool Unsubscribe(object subscriber, string eventName) {
            if (!map.ContainsKey(eventName)) { return false; }
            Delegate _;
            if (map[eventName].TryRemove(subscriber, out _)) {
                if (map[eventName].IsEmpty) return TryRemove(map, eventName);
                return true;
            }
            Log.w("Could not unsubscribe subscriber=" + subscriber + " from event '" + eventName + "'");
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