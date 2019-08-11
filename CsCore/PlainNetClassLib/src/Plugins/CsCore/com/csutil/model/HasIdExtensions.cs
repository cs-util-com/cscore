using System.Threading.Tasks;

namespace com.csutil.model {

    public static class HasIdExtensions {

        public static Task<T> SaveToDb<T>(this T self) where T : HasId {
            var store = IoC.inject.Get<HasIdDbConnection<T>>(self);
            if (store == null) { throw new InjectionException("No HasIdStore registered for type=" + typeof(T)); }
            return store.Save(self);
        }

        public static Task<T> LoadFromDb<T>(this T defaultValue, string key) where T : HasId {
            var store = IoC.inject.Get<HasIdDbConnection<T>>(defaultValue);
            if (store == null) { throw new InjectionException("No HasIdStore registered for type=" + typeof(T)); }
            return store.Load(key, defaultValue);
        }

    }

}