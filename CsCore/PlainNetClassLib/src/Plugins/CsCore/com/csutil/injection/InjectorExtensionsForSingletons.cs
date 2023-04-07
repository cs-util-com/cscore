using System;
using System.Diagnostics;
using System.Linq;
using com.csutil.injection;

namespace com.csutil {

    public static class Singleton {

        private static object syncLock = new object();

        public static object SetSingleton<V>(this Injector self, V singletonInstance, bool overrideExisting = false) {
            return self.SetSingleton<V, V>(new object(), singletonInstance, overrideExisting);
        }

        public static V SetSingleton<V>(this Injector self, object caller, V singletonInstance, bool overrideExisting = false) {
            self.SetSingleton<V, V>(caller, singletonInstance, overrideExisting);
            return singletonInstance;
        }

        // private because normally prefer GetOrAddSingleton should be used instead
        private static object SetSingleton<T, V>(this Injector self, object caller, V singletonInstance, bool overrideExisting = false) where V : T {
            lock (syncLock) {
                singletonInstance.ThrowErrorIfNull("singletonInstance");
                if (!overrideExisting && typeof(T).IsCastableTo<IDisposableV2>()) {
                    // Cause a injector cleanup if there is a present singleton and this singleton is disposed:
                    self.Get<T>(caller, false); // Afterwards self.HasInjectorRegistered will be false if the singleton was disposed
                }
                if (self.HasInjectorRegistered<T>(out var existingInjectors)) {
                    if (!overrideExisting) {
                        if (self.TryGetCreationStackTraceFor<T>(existingInjectors.First(), out StackTrace stackTrace)) {
                            var existingSingleton = stackTrace.ToException("Creation stacktrace of existing Singleton that blocked the SetSingleton call:");
                            throw new InvalidOperationException("Existing provider found for " + typeof(T), existingSingleton);
                        } else {
                            throw new InvalidOperationException("Existing provider found for " + typeof(T));
                        }
                    }
                    if (!self.RemoveAllInjectorsFor<T>()) { Log.e("Could not remove all existing injectors!"); }
                    return SetSingleton<T, V>(self, caller, singletonInstance, false); // then retry setting the singleton
                }
                self.RegisterInjector<T>(caller, delegate {
                    if (singletonInstance is IDisposableV2 disposableObj && !disposableObj.IsAlive()) {
                        // If the found object is not active anymore destroy the singleton injector and return null
                        if (!self.RemoveAllInjectorsFor<T>()) { Log.e("Could not remove all existing injectors!"); }
                        return default;
                    }
                    return singletonInstance;
                });
                return caller;
            }
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller) {
            return GetOrAddSingleton(self, caller, () => CreateNewInstance<T>());
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller, Func<T> createSingletonInstance) {
            lock (syncLock) {
                if (self.TryGet(caller, out T singleton)) {
                    AssertNotNull(singleton);
                    return singleton;
                }
                singleton = createSingletonInstance();
                AssertNotNull(singleton);
                return self.SetSingleton(caller, singleton);
            }
        }

        public static void RemoveSingleton<V>(this Injector self, V singletonInstance) {
            var i = self.Get<V>(null);
            if (!ReferenceEquals(i, singletonInstance)) {
                throw new InvalidOperationException("Could not remove singleton " + singletonInstance + " because it was not the current singleton");
            }
            self.RemoveAllInjectorsFor<V>();
        }

        [Conditional("DEBUG")]
        private static void AssertNotNull<T>(T singleton) {
            if (ReferenceEquals(null, singleton)) {
                throw new ArgumentNullException("The singleton instance was null for type " + typeof(T));
            }
            if ("null".Equals(singleton.ToString())) {
                throw new ArgumentNullException("The singleton instance returns 'null' in .ToString() for type " + typeof(T));
            }
        }

        private static T CreateNewInstance<T>() {
            return (T)Activator.CreateInstance(typeof(T));
        }

    }

}