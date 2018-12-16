using System;
using System.Diagnostics;
using com.csutil.logging;

namespace com.csutil {
    public class LogViaConsole : ILog {
        public void log(string msg, params object[] args) {
            Console.WriteLine(msg, args);
        }

        public void logWarning(string warning, params object[] args) {
            Console.WriteLine("> WARNING: " + warning, args);
        }

        public Exception logError(string error, params object[] args) {
            return logExeption(new Exception(">>> ERROR: " + error), args);
        }

        public Exception logExeption(Exception e, params object[] args) {
            Console.WriteLine(">>> " + e, args);
            return e;
        }

    }
}