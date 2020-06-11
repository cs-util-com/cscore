using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class TypeExtensions {

        public static bool IsSubclassOf<T>(this Type self) {
            Type t = typeof(T);
            return t == self || self.IsSubclassOf(t);
        }

        public static bool IsCastableTo<T>(this Type self) {
            return self.IsCastableTo(typeof(T));
        }

        public static bool IsCastableTo(this Type self, params Type[] types) {
            return types.Any(t => (t == self || t.IsAssignableFrom(self)));
        }

        // From https://stackoverflow.com/a/863944/165106
        public static bool IsPrimitiveOrSimple(this Type t) {
            var i = t.GetTypeInfo();
            // nullable type, check if the nested type is simple:
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Nullable<>)) { return IsPrimitiveOrSimple(i.GetGenericArguments()[0]); }
            return i.IsPrimitive || i.IsEnum || t.Equals(typeof(string)) || t.Equals(typeof(decimal));
        }

        public static bool IsSystemType(this Type type) { return type.Assembly == typeof(object).Assembly; }

    }

    public static class TypeCheck {
        public static bool AreEqual<T, V>() { return typeof(T) == typeof(V); }
    }

}
