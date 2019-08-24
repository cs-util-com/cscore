using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class ReflectionExtensions {

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly) {
            try { return assembly.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        public static bool HasAttribute<T>(this MemberInfo self, bool inherit = false) where T : Attribute {
            return self.GetCustomAttributes(typeof(T), inherit).Any();
        }

        public static string ToStringV2(this MemberInfo m) { return m.DeclaringType.Name + "." + m.Name; }

    }
}
