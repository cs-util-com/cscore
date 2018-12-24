using System;
using System.Runtime.Serialization;
using com.csutil.injection;

namespace com.csutil {

    public static class Singleton {

        public static object SetSingleton<V>(this Injector self, V singletonInstance) { return self.SetSingleton<V, V>(singletonInstance); }

        public static object SetSingleton<T, V>(this Injector self, V singletonInstance) where V : T {
            if (self.HasInjectorRegistered<T>()) { throw new MultipleProvidersException("Existing provider found for " + typeof(T)); }
            var injectorRef = new object();
            self.RegisterInjector<T>(injectorRef, (caller, createIfNull) => { return singletonInstance; });
            return injectorRef;
        }

        public static T GetOrAddSingleton<T>(this Injector self, object caller) {
            T singleton = self.Get<T>(caller, true);
            if (singleton == null) {
                singleton = createNewInstance<T>();
                if (ReferenceEquals(null, singleton) || "null".Equals("" + singleton)) {
                    throw new Exception("Could not instantiate " + typeof(T));
                }
                self.SetSingleton(singleton);
            }
            return singleton;
        }

        private static T createNewInstance<T>() { return (T)Activator.CreateInstance(typeof(T)); }

        public class MultipleProvidersException : Exception { public MultipleProvidersException(string message) : base(message) { } }

    }

}