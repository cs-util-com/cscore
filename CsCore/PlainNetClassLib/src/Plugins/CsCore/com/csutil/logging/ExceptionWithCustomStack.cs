using System;
using System.Diagnostics;

namespace com.csutil {

    public class Error : Exception {

        public StackTrace stack;

        public Error(string message, StackTrace stacktrace) : base(message) {
            stack = stacktrace;
        }

        // Required default constructors (https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1032):
        public Error() { }
        public Error(string message) : base(message) { }
        public Error(string message, Exception innerException) : base(message, innerException) { }

        public override string StackTrace => stack.ToString();

    }

}