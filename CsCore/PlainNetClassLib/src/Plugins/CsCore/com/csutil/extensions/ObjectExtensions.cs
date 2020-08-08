using System;
using System.Runtime.CompilerServices;

namespace com.csutil {

    public static class ObjectExtensions {

        public static void ThrowErrorIfNull(this object self, string paramName, [CallerMemberName] string methodName = null) {
            if (self == null) {
                throw new ArgumentNullException($"{paramName} (Method {methodName})");
            }
        }

    }

}