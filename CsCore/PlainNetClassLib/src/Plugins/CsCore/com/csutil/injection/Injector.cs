using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.eventbus;

namespace com.csutil.injection {
    public class Injector {

        public static Injector newInjector(IEventBus eventbusToUse) { return new Injector { usedEventBus = eventbusToUse }; }

        public IEventBus usedEventBus = EventBus.instance;

        public void RegisterInjector<T>(object injector, Func<object, bool, T> createServiceAction) {
            AssertV2.IsFalse(HasInjectorRegistered<T>(), "There are already injectors registered for " + GetEventKey<T>());
            usedEventBus.Subscribe(injector, GetEventKey<T>(), createServiceAction);
        }

        public T Get<T>(object caller, bool createIfNull = true) {
            var results = usedEventBus.Publish(GetEventKey<T>(), caller, createIfNull).Filter(x => x is T).Cast<T>();
            if (results.IsNullOrEmpty()) { Log.w("No inject results for " + GetEventKey<T>()); }
            if (results.Count() < 2) { Log.w("Multiple injectors set for " + GetEventKey<T>()); }
            return results.FirstOrDefault();
        }

        private string GetEventKey<T>() { return "InjectReq:" + typeof(T); }

        public bool HasInjectorRegistered<T>() { return usedEventBus.HasListenersFor(GetEventKey<T>()); }

        public bool UnregisterInjector<T>(object injector) { return usedEventBus.Unsubscribe(injector, GetEventKey<T>()); }

        public bool UnregisterAllInjectors<T>() { return usedEventBus.UnsubscribeAll(GetEventKey<T>()); }

    }
}