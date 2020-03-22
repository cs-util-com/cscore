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

        /// <summary> If true all erros during publish are not only logged but rethrown. Will be true in DEBUG mode </summary>
        public bool throwPublishErrors = false;

        /// <summary> sync subscribing and publishing to not happen at the same time </summary>
        private object threadLock = new object();

        private readonly ConcurrentDictionary<string, List<KeyValuePair<object, Delegate>>> map =
            new ConcurrentDictionary<string, List<KeyValuePair<object, Delegate>>>();

        public EventBus() {
            eventHistory = new ConcurrentQueue<string>();
#if DEBUG // In debug mode throw all publish errors:
            throwPublishErrors = true;
#endif
        }

        public void Subscribe(object subscriber, string eventName, Delegate callback) {
            lock (threadLock) {
                var replacedDelegate = AddOrReplace(GetOrAdd(eventName), subscriber, callback);
                if (replacedDelegate != null) { Log.w("Existing subscriber was replaced for event=" + eventName); }
            }
        }

        private Delegate AddOrReplace(List<KeyValuePair<object, Delegate>> self, object subscriber, Delegate callback) {
            var i = self.FindIndex(x => x.Key == subscriber);
            var newEntry = new KeyValuePair<object, Delegate>(subscriber, callback);
            if (i >= 0) {
                var oldEntry = self[i];
                self[i] = newEntry;
                return oldEntry.Value;
            } else {
                self.Add(newEntry);
                return null;
            }
        }

        private List<KeyValuePair<object, Delegate>> GetOrAdd(string eventName) {
            return map.GetOrAdd(eventName, (_) => { return new List<KeyValuePair<object, Delegate>>(); });
        }

        public IEnumerable<object> GetSubscribersFor(string eventName) {
            var subscribers = map.GetValue(eventName, null);
            if (subscribers != null) {
                var m = subscribers.Map(x => x.Key);
                return m;
            }
            return new List<object>();
        }

        public List<object> Publish(string eventName, params object[] args) {
            return NewPublishIEnumerable(eventName, args).ToList();
        }

        public IEnumerable<object> NewPublishIEnumerable(string eventName, params object[] args) {
            lock (threadLock) {
                eventHistory.Enqueue(eventName);
                List<KeyValuePair<object, Delegate>> dictForEventName;
                map.TryGetValue(eventName, out dictForEventName);
                if (!dictForEventName.IsNullOrEmpty()) {
                    var subscriberDelegates = dictForEventName.Map(x => x.Value).ToList();
                    return subscriberDelegates.Map(subscriberDelegate => {
                        try {
                            object result;
                            if (subscriberDelegate.DynamicInvokeV2(args, out result, throwPublishErrors)) { return result; }
                        } catch (Exception e) { if (throwPublishErrors) { throw; } else { Log.e(e); } }
                        return null;
                    });
                }
                return new List<object>();
            }
        }

        public bool Unsubscribe(object subscriber, string eventName) {
            if (!map.ContainsKey(eventName)) { return false; }
            KeyValuePair<object, Delegate> elemToRemove = map[eventName].FirstOrDefault(x => x.Key == subscriber);
            if (map[eventName].Remove(elemToRemove)) {
                if (map[eventName].IsNullOrEmpty()) return TryRemove(map, eventName);
                return true;
            }
            Log.w("Could not unsubscribe subscriber=" + subscriber + " from event '" + eventName + "'");
            return false;
        }

        public bool UnsubscribeAll(object subscriber) {
            var registeredEvents = map.Filter(x => x.Value.Exists(y => y.Key == subscriber));
            var removedCallbacks = new List<Delegate>();
            foreach (var eventMaps in registeredEvents) {
                var eventName = eventMaps.Key;
                var subscribersForEventName = eventMaps.Value;
                var entryToRemove = subscribersForEventName.First(x => x.Key == subscriber);
                if (subscribersForEventName.Remove(entryToRemove)) {
                    removedCallbacks.Add(entryToRemove.Value);
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