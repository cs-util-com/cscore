using System;
using com.csutil.injection;

namespace com.csutil {

    public static class Singleton {

        private static object syncLock = new object();

        public static object SetSingleton<V>(this Injector self, V singletonInstance, bool overrideExisting = false) {
            return self.SetSingleton<V, V>(singletonInstance, overrideExisting);
        }

        public static object SetSingleton<T, V>(this Injector self, V singletonInstance, bool overrideExisting = false) where V : T {
            lock (syncLock) {
                if (self.HasInjectorRegistered<T>()) {
                    if (!overrideExisting) { throw new MultipleProvidersException("Existing provider found for " + typeof(T)); }
                    if (!self.RemoveAllInjectorsFor<T>()) { Log.e("Could not remove all existing injectors!"); }
                    return SetSingleton<T, V>(self, singletonInstance, false); // then retry setting the singleton
                }
                var injectorRef = new object();
                self.RegisterInjector<T>(injectorRef, (caller, createIfNull) => { return singletonInstance; });
                return injectorRef;
            }
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller) {
            return GetOrAddSingleton(self, caller, () => CreateNewInstance<T>());
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller, Func<T> createSingletonInstance) {
            lock (syncLock) {
                T singleton = self.Get<T>(caller, true);
                if (singleton == null) {
                    singleton = createSingletonInstance();
                    if (ReferenceEquals(null, singleton) || "null".Equals("" + singleton)) {
                        throw new Exception("Could not instantiate " + typeof(T));
                    }
                    self.SetSingleton(singleton);
                }
                return singleton;
            }
        }

        private static T CreateNewInstance<T>() {
            return (T)Activator.CreateInstance(typeof(T));
        }

        [Serializable]
        public class MultipleProvidersException : Exception {
            public MultipleProvidersException(string message) : base(message) { }
        }

    }

}