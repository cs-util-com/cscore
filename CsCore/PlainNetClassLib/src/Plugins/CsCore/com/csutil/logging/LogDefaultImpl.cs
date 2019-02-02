using System;
using com.csutil.logging;

namespace com.csutil {

    public abstract class LogDefaultImpl : ILog {

        private const string LB = "\r\n"; // LineBreak

        public void LogDebug(string msg, params object[] args) {
            PrintDebugMessage("> " + msg + Log.ToArgsStr(args, ToString) + LB
                + "  at " + Log.CallingMethodStr(args) + LB + LB);
        }

        public void LogWarning(string warning, params object[] args) {
            PrintWarningMessage("> WARNING: " + warning + Log.ToArgsStr(args, ToString) + LB
                + "  at " + Log.CallingMethodStr(args) + LB + LB);
        }

        public Exception LogError(string error, params object[] args) {
            PrintErrorString(">>> ERROR: " + error, args);
            return new Exception(error);
        }

        public Exception LogExeption(Exception e, params object[] args) {
            PrintException(e, args);
            return e;
        }

        private void PrintErrorString(string e, object[] args) {
            PrintErrorMessage(e + Log.ToArgsStr(args, ToString) + LB
                + "    at " + Log.CallingMethodStr(args) + LB + LB);
        }

        protected abstract void PrintDebugMessage(string debugLogMsg, params object[] args);
        protected abstract void PrintWarningMessage(string warningMsg, params object[] args);
        protected abstract void PrintErrorMessage(string errorMsg, params object[] args);
        protected virtual void PrintException(Exception e, object[] args) {
            // The default implementation prints exceptions the same as errors:
            PrintErrorString(">>> EXCEPTION: " + e, args);
        }

        protected abstract string ToString(object arg);

    }

}