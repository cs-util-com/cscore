using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class TypeExtensions {

        public static bool IsSubclassOf<T>(this Type self) { return self.IsSubclassOf(typeof(T)); }
        public static bool IsAssignableFrom<T>(this Type self) { return self.IsAssignableFrom(typeof(T)); }

    }

    public static class TypeCheck {
        public static bool AreEqual<T, V>() { return typeof(T) == typeof(V); }
    }

}
