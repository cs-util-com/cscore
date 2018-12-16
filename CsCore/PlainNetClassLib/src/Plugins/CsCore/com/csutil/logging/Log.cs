using System;
using System.Diagnostics;
using com.csutil.logging;

namespace com.csutil {
    public class Log {
        public static ILog instance = new LogViaConsole();

        [Conditional("DEBUG")]
        public static void d(string msg, params object[] args) {
            instance.log(msg, args);
        }

        [Conditional("DEBUG")]
        public static void w(string warning, params object[] args) {
            instance.logWarning(warning, args);
        }

        public static Exception e(string error, params object[] args) {
            return instance.logError(error, args);
        }

        public static Exception e(Exception e, params object[] args) {
            return instance.logExeption(e, args);
        }

    }
}