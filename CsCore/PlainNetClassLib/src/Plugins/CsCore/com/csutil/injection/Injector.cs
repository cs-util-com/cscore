using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace com.csutil.injection {
    public class Injector {

        public static Injector newInjector(IEventBus eventbusToUse) { return new Injector { usedEventBus = eventbusToUse }; }

        public IEventBus usedEventBus = EventBus.instance;

        public void RegisterInjector<T>(object injector, Func<object, bool, T> createServiceAction) {
            if (HasInjectorRegistered<T>()) { Log.w("There are already injectors registered for " + GetEventKey<T>()); }
            usedEventBus.Subscribe(injector, GetEventKey<T>(), createServiceAction);
        }

        public T Get<T>(object caller, bool createIfNull = true) {
            return GetAll<T>(caller, createIfNull).FirstOrDefault();
        }

        public IEnumerable<T> GetAll<T>(object caller, bool createIfNull = true) {
            return usedEventBus.NewPublishIEnumerable(GetEventKey<T>(), caller, createIfNull).Filter(x => x is T).Cast<T>();
        }

        public string GetEventKey<T>() { return "InjectReq:" + typeof(T); }

        public bool HasInjectorRegistered<T>() {
            return !usedEventBus.GetSubscribersFor(GetEventKey<T>()).IsNullOrEmpty();
        }

        public bool RemoveAllInjectorsFor<T>() {
            var eventName = GetEventKey<T>();
            var subscribers = usedEventBus.GetSubscribersFor(eventName).ToList();
            if (subscribers.IsNullOrEmpty()) {
                Log.w("Could not remove subscribers because there were none found for '" + eventName + "'");
                return false;
            }
            var r = true;
            foreach (var subscriber in subscribers) { r = usedEventBus.Unsubscribe(subscriber, eventName) & r; }
            Log.d("Removed " + subscribers.Count() + " subscribers for " + typeof(T));
            return r;
        }

        public bool UnregisterInjector<T>(object injector) { return usedEventBus.Unsubscribe(injector, GetEventKey<T>()); }

        /// <summary> sets up a temporary context in which injecting the defined Type returns the set myContextInstance </summary>
        public void DoWithTempContext<T>(T myContextInstance, Action runWithTemporaryContext) {
            var injector = new object();
            var threadOfContextCreation = Thread.CurrentThread;
            RegisterInjector<T>(injector, (requester, createIfNull) => {
                if (Thread.CurrentThread == threadOfContextCreation) { return myContextInstance; }
                return default(T);
            });
            runWithTemporaryContext();
            UnregisterInjector<T>(injector);
        }

    }
}