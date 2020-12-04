using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace com.csutil {

    public static class ObjectExtensions {

        public static void ThrowErrorIfNull(this object self, string paramName, [CallerMemberName] string methodName = null) {
            if (self == null) {
                throw new ArgumentNullException($"{paramName} (Method {methodName})");
            }
        }

        public static void ThrowErrorIfNull(this object self, Func<Exception> onNull) { if (self == null) { throw onNull(); } }
        public static async Task ThrowErrorIfNull(this object self, Func<Task<Exception>> onNull) {
            if (self == null) { throw await onNull(); }
        }

    }

}