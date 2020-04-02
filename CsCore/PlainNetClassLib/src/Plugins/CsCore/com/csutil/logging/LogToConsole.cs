using System;

namespace com.csutil.logging {

    public class LogToConsole : LogDefaultImpl {

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            WithColor(ConsoleColor.DarkGray, () => { Console.Write(debugLogMsg); });
        }

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            WithColor(ConsoleColor.Red, () => { Console.Write(errorMsg); });
        }

        protected override void PrintWarningMessage(string warningMsg, params object[] args) {
            WithColor(ConsoleColor.Yellow, () => { Console.Write(warningMsg); });
        }

        private void WithColor(ConsoleColor newForegroundColor, Action WriteToConsoleAction) {
            var oldForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = newForegroundColor;
            WriteToConsoleAction();
            Console.ForegroundColor = oldForegroundColor;
        }

    }

}