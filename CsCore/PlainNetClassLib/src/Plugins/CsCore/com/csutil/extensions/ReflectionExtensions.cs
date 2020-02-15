using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class ReflectionExtensions {

        public static IEnumerable<Type> GetLoadableTypes(this Assembly self) {
            try { return self.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        public static IEnumerable<string> GetTypesWithMissingNamespace(this Assembly self) {
            return self.GetLoadableTypes().Filter(x => x.Namespace.IsNullOrEmpty()).Map(x => x.FullName);
        }

        public static IOrderedEnumerable<string> GetAllNamespaces(this Assembly self) {
            return self.GetLoadableTypes().Map(x => x.Namespace).Distinct().OrderBy(x => x);
        }

        public static bool HasAttribute<T>(this MemberInfo self, bool inherit = false) where T : Attribute {
            return self.GetCustomAttributes(typeof(T), inherit).Any();
        }

        public static string ToStringV2(this MemberInfo m) { return m.DeclaringType.Name + "." + m.Name; }

    }
}
