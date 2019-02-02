using System;
using System.Diagnostics;
using com.csutil.logging;

namespace com.csutil {

    public class LogViaConsole : LogDefaultImpl {

        internal override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            Console.Write(debugLogMsg);
        }

        internal override void PrintErrorMessage(string errorMsg, params object[] args) {
            Console.Write(errorMsg);
        }

        internal override void PrintWarningMessage(string warningMsg, params object[] args) {
            Console.Write(warningMsg);
        }

        internal override string ToString(object arg) {
            if (arg is StackFrame) { return null; }
            return "" + arg;
        }

    }

}