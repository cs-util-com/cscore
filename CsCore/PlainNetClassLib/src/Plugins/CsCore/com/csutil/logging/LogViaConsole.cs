using System;
using System.Diagnostics;
using com.csutil.logging;

namespace com.csutil {

    public class LogViaConsole : ILog {

        private const string LB = "\r\n";

        public void LogDebug(string msg, params object[] args) {
            Console.Write("> " + msg + LB + "  at " + Log.CallingMethodStr(args) + LB + LB, args);
        }

        public void LogWarning(string warning, params object[] args) {
            Console.Write("> WARNING: " + warning + LB + "  at " + Log.CallingMethodStr(args) + LB + LB, args);
        }

        public Exception LogError(string error, params object[] args) {
            printExceptionString(">>> ERROR: " + error, args);
            return new Exception(error);
        }

        public Exception LogExeption(Exception e, params object[] args) {
            printExceptionString(">>> EXCEPTION: " + e, args);
            return e;
        }

        private static void printExceptionString(string e, object[] args) {
            Console.Write(e + LB + "    at " + Log.CallingMethodStr(args) + LB + LB, args);
        }

    }

}