using System;
using System.Collections.Generic;
using System.Linq;
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

    }

    public static class TypeCheck {
        public static bool AreEqual<T, V>() { return typeof(T) == typeof(V); }
    }

}
