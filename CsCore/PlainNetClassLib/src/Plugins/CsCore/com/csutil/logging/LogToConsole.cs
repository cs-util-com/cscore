using System;
using System.Diagnostics;

namespace com.csutil.logging {

    public class LogToConsole : LogDefaultImpl {

        protected override void PrintDebugMessage(string debugLogMsg, object[] args) {
            Console.Write(debugLogMsg);
        }

        protected override void PrintErrorMessage(string errorMsg, object[] args) {
            Console.Write(errorMsg);
        }

        protected override void PrintWarningMessage(string warningMsg, object[] args) {
            Console.Write(warningMsg);
        }

    }

}