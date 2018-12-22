using System;
using System.Diagnostics;
using System.Linq;
using com.csutil.logging;

namespace com.csutil {

    public static class Log {
        public static ILog instance = new LogViaConsole();

        [Conditional("DEBUG")]
        public static void d(string msg, params object[] args) {
            instance.LogDebug(msg, args);
        }

        [Conditional("DEBUG")]
        public static void w(string warning, params object[] args) {
            instance.LogWarning(warning, args);
        }

        public static Exception e(string error, params object[] args) {
            return instance.LogError(error, args);
        }

        public static Exception e(Exception e, params object[] args) {
            return instance.LogExeption(e, args);
        }

        public static string CallingMethodName(object[] args = null, int i = 3) {
            StackFrame f = args?.FirstOrDefault(x => x is StackFrame) as StackFrame;
            if (f == null) { f = new StackTrace().GetFrame(i); }
            return f.GetMethodName();
        }

    }

    public static class LogExtensions {

        public static Exception PrintStackTrace(this Exception self, params object[] args) {
            return Log.e(self, args);
        }

        /// <summary> Will return a formated string in the form of ClassName.MethodName </summary>
        public static string GetMethodName(this StackFrame self) {
            try {
                var method = self.GetMethod(); // analyse stack trace for class name:
                var paramsString = method.GetParameters().ToStringV2(x => "" + x, "", "");
                return method.ReflectedType.Name + "." + method.Name + "(" + paramsString + ")";
            } catch (Exception e) { Console.WriteLine("" + e); return ""; }
        }

    }

}