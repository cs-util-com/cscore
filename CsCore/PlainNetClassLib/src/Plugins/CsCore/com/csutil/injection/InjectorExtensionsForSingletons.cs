using System;
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
                if (self.HasInjectorRegistered<T>()) {
                    if (!overrideExisting) { throw new MultipleProvidersException("Existing provider found for " + typeof(T)); }
                    if (!self.RemoveAllInjectorsFor<T>()) { Log.e("Could not remove all existing injectors!"); }
                    return SetSingleton<T, V>(self, caller, singletonInstance, false); // then retry setting the singleton
                }
                self.RegisterInjector<T>(caller, delegate { return singletonInstance; });
                return caller;
            }
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller) {
            return GetOrAddSingleton(self, caller, () => CreateNewInstance<T>());
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller, Func<T> createSingletonInstance) {
            lock (syncLock) {
                T singleton = self.Get<T>(caller, true);
                if (singleton != null) { return singleton; }
                singleton = createSingletonInstance();
                if (ReferenceEquals(null, singleton) || "null".Equals("" + singleton)) {
                    throw new ArgumentNullException("The created singleton instance was null for type " + typeof(T));
                }
                return self.SetSingleton(caller, singleton);
            }
        }

        private static T CreateNewInstance<T>() {
            return (T)Activator.CreateInstance(typeof(T));
        }

        [Serializable]
        public class MultipleProvidersException : Exception {
            public MultipleProvidersException() : base() { }
            public MultipleProvidersException(string message) : base(message) { }
            public MultipleProvidersException(string message, Exception innerException) : base(message, innerException) { }
        }

    }

}