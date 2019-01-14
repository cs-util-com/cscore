using com.csutil;
using System.Linq;

namespace System {
    public static class DelegateExtensions {

        public static bool InvokeIfNotNull(this Delegate self, params object[] passedParams) {
            if (self == null) { return false; }
            var res = InvokeIfNotNull<object>(self, passedParams);
            if (res is bool) { return (bool)res; }
            return true;
        }

        public static T InvokeIfNotNull<T>(this Delegate self, params object[] passedParams) {
            if (self != null) {
                object result;
                if (DynamicInvokeV2(self, passedParams, out result)) { return (T)result; }
            }
            return default(T);
        }

        public static object DynamicInvokeV2(this Delegate self, params object[] passedParams) {
            object result;
            DynamicInvokeV2(self, passedParams, out result);
            return result;
        }

        public static bool DynamicInvokeV2(this Delegate self, object[] passedParams, out object result) {
            var methodParams = self.Method.GetParameters();
            if (methodParams.Length == passedParams.Length) {
                result = (self.DynamicInvoke(passedParams));
                return true;
            } else if (methodParams.Length < passedParams.Length) {
                var subset = passedParams.Take(methodParams.Length).ToArray();
                result = (self.DynamicInvoke(subset));
                return true;
            } else {
                Log.w("Listener skipped because not enough parameters passed: " + self);
            }
            result = null;
            return false;
        }

    }
}