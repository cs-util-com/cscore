using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.injection {
    public class Injector {

        public static Injector newInjector(IEventBus eventbusToUse) { return new Injector { usedEventBus = eventbusToUse }; }

        public IEventBus usedEventBus = EventBus.instance;

        public void RegisterInjector<T>(object injector, Func<object, bool, T> createServiceAction) {
            if (HasInjectorRegistered<T>()) { Log.w("There are already injectors registered for " + GetEventKey<T>()); }
            usedEventBus.Subscribe(injector, GetEventKey<T>(), createServiceAction);
        }

        public T Get<T>(object caller, bool createIfNull = true) {
            IEnumerable<T> results = GetAll<T>(caller, createIfNull);
            if (results.Count() > 2) { Log.w("Multiple injectors set for " + GetEventKey<T>()); }
            return results.FirstOrDefault();
        }

        public IEnumerable<T> GetAll<T>(object caller, bool createIfNull = true) {
            var results = usedEventBus.Publish(GetEventKey<T>(), caller, createIfNull).Filter(x => x is T).Cast<T>();
            // if (results.IsNullOrEmpty()) { Log.d("No inject results for " + GetEventKey<T>()); }
            return results;
        }

        private string GetEventKey<T>() { return "InjectReq:" + typeof(T); }

        public bool HasInjectorRegistered<T>() {
            return !usedEventBus.GetSubscribersFor(GetEventKey<T>()).IsNullOrEmpty();
        }

        public bool RemoveAllInjectorsFor<T>() {
            var eventName = GetEventKey<T>();
            var subscribers = usedEventBus.GetSubscribersFor(eventName);
            if (subscribers.IsNullOrEmpty()) {
                Log.w("Could not remove subscribers because there were none found for '" + eventName + "'");
                return false;
            }
            var r = true;
            foreach (var subscriber in subscribers) { r = usedEventBus.Unsubscribe(subscriber, eventName) & r; }
            Log.d("Removed " + subscribers.Count + " subscribers for " + typeof(T));
            return r;
        }

        public bool UnregisterInjector<T>(object injector) { return usedEventBus.Unsubscribe(injector, GetEventKey<T>()); }

    }
}