using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace com.csutil.injection {
    public class Injector {

        public static Injector newInjector(IEventBus eventbusToUse) { return new Injector { usedEventBus = eventbusToUse }; }

        public IEventBus usedEventBus = EventBus.instance;
        public IImmutableSet<string> injectorNames { get; private set; } = ImmutableHashSet<string>.Empty;

        private IImmutableDictionary<InjectorKey, StackTrace> creationStackTraces = ImmutableDictionary<InjectorKey, StackTrace>.Empty;

        public void RegisterInjector<T>(object injector, Func<object, bool, T> createServiceAction) {
            var injectorName = GetEventKey<T>();
            injectorNames = injectorNames.Add(injectorName);
            if (injector != null) {
                creationStackTraces = creationStackTraces.SetItem(InjectorKey.Get<T>(injector), new StackTrace(1, true));
            }
            if (HasInjectorRegistered<T>()) { Log.w("There are already injectors registered for " + injectorName); }
            usedEventBus.Subscribe(injector, injectorName, createServiceAction);
            EventBus.instance.Publish(EventConsts.catInjection, "Register_" + injectorName);
        }

        public bool TryGet<T>(object caller, out T obj, bool createIfNull = true) {
            if (HasInjectorRegistered<T>()) {
                obj = Get<T>(caller, createIfNull);
                return obj != null;
            }
            obj = default(T);
            return false;
        }

        public T Get<T>(object caller, bool createIfNull = true) {
            return GetAll<T>(caller, createIfNull).FirstOrDefault();
        }

        public IEnumerable<T> GetAll<T>(object caller, bool createIfNull = true) {
            return GetAll(GetEventKey<T>(), caller, createIfNull)
                .Filter(x => x is T && (!(x is IDisposableV2) || (x is IDisposableV2 d && d.IsAlive(logErrorIfDisposed: true))))
                .Cast<T>();
        }

        private IEnumerable<object> GetAll(string injectorKey, object caller, bool createIfNull) {
            return usedEventBus.NewPublishIEnumerable(injectorKey, caller, createIfNull);
        }

        public string GetEventKey<T>() { return GetEventKey(typeof(T)); }
        public static string GetEventKey(Type type) { return "InjectReq:" + type; }

        public bool HasInjectorRegistered<T>() {
            var existingInjectors = usedEventBus.GetSubscribersFor(GetEventKey<T>());
            return !existingInjectors.IsNullOrEmpty();
        }

        public bool HasInjectorRegistered<T>(out IEnumerable<object> existingInjectors) {
            existingInjectors = usedEventBus.GetSubscribersFor(GetEventKey<T>());
            return !existingInjectors.IsNullOrEmpty();
        }

        public Dictionary<string, IEnumerable<object>> GetAllInjectorsMap(object caller, bool createIfNull = false) {
            return injectorNames.ToDictionary(n => n, n => GetAll(n, caller, createIfNull));
        }

        public bool RemoveAllInjectorsFor<T>() { return RemoveAllInjectorsFor(typeof(T)); }

        public bool RemoveAllInjectorsFor(Type type) {
            var eventName = GetEventKey(type);
            var injectors = usedEventBus.GetSubscribersFor(eventName).ToList();
            if (injectors.IsNullOrEmpty()) {
                return true;
            }
            var r = true;
            foreach (var injector in injectors) {
                r = usedEventBus.Unsubscribe(injector, eventName) && r;
                TryRemoveCreationStackTrace(injector, type);
            }
            injectorNames = injectorNames.Remove(eventName);
            // Log.d("Removed " + subscribers.Count() + " subscribers for " + typeof(T));
            return r;
        }

        public bool UnregisterInjector<T>(object injector) { return UnregisterInjector(injector, typeof(T)); }

        public bool UnregisterInjector(object injector, Type type) {
            var eventName = GetEventKey(type);
            if (usedEventBus.Unsubscribe(injector, eventName)) {
                if (usedEventBus.GetSubscribersFor(eventName).IsNullOrEmpty()) {
                    injectorNames = injectorNames.Remove(eventName);
                }
                TryRemoveCreationStackTrace(injector, type);
                return true;
            }
            return false;
        }

        private void TryRemoveCreationStackTrace(object injector, Type type) {
            if (injector == null || type == null) { return; }
            var injectorKey = InjectorKey.Get(injector, type);
            if (creationStackTraces.ContainsKey(injectorKey)) {
                creationStackTraces = creationStackTraces.Remove(injectorKey);
            } else {
                Log.w("Could not find entry in creationStackTraces for " + injectorKey);
            }
        }

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

        public bool TryGetCreationStackTraceFor<T>(object injector, out StackTrace stackTrace) {
            return creationStackTraces.TryGetValue(InjectorKey.Get<T>(injector), out stackTrace);
        }

        private struct InjectorKey : IEquatable<InjectorKey> {

            public static InjectorKey Get<T>(object o) { return new InjectorKey(o, typeof(T)); }
            public static InjectorKey Get(object o, Type type) { return new InjectorKey(o, type); }

            readonly Type Type;
            readonly object Injector;

            private InjectorKey(object injector, Type type) {
                type.ThrowErrorIfNull("type");
                injector.ThrowErrorIfNull("injector");
                Type = type;
                Injector = injector;
            }

            public override bool Equals(object obj) {
                if (obj is InjectorKey other) { return Type == other.Type && Injector == other.Injector; }
                return false;
            }

            public override int GetHashCode() { return Type.GetHashCode() + Injector.GetHashCode(); }

            public bool Equals(InjectorKey other) { return Type == other.Type && Injector == other.Injector; }

            public override string ToString() { return $"{Injector}<{Type}>"; }

        }

    }

}