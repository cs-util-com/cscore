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
            return self.GetLoadableTypes().Filter(x => x.Namespace.IsNullOrEmpty()).Map(x => $"{x.FullName} (Assembly {x.Assembly.FullName})");
        }

        public static IOrderedEnumerable<string> GetAllNamespaces(this Assembly self) {
            return self.GetLoadableTypes().Map(x => x.Namespace).Distinct().OrderBy(x => x);
        }

        public static bool HasAttribute<T>(this MemberInfo self, bool inherit = false) where T : Attribute {
            return self.IsDefined(typeof(T), inherit);
        }

        public static bool TryGetCustomAttribute<T>(this MemberInfo self, out T attr, bool inherit = false) where T : Attribute {
            attr = self.GetCustomAttribute<T>(inherit);
            return attr != null;
        }

        public static bool TryGetCustomAttributes<T>(this MemberInfo self, out IEnumerable<T> attr, bool inherit = false) where T : Attribute {
            attr = self.GetCustomAttributes<T>(inherit);
            return !attr.IsNullOrEmpty();
        }

        public static bool CanWriteTo(this MemberInfo self) {
            if (self is FieldInfo f) { return !f.Attributes.ContainsFlag(FieldAttributes.InitOnly); }
            if (self is PropertyInfo p) { return p.CanWrite && p.GetSetMethod(nonPublic: true).IsPublic; }
            return false;
        }

        public static object GetValue(this MemberInfo self, object obj) {
            if (self is PropertyInfo p) { return p.GetValue(obj); }
            if (self is FieldInfo f) { return f.GetValue(obj); }
            if (self is MethodInfo m) { return m.Invoke(obj, null); }
            return null;
        }

        public static string ToStringV2(this MemberInfo m) {
            if (m.DeclaringType == null) { return m.Name; }
            return m.DeclaringType.Name + "." + m.Name;
        }

    }
}
