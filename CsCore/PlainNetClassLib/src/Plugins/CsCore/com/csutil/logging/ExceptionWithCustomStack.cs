using System;
using System.Diagnostics;

namespace com.csutil {

    public class Error : Exception {

        public StackTrace stack;

        public Error(string message, StackTrace stacktrace) : base(message) {
            stack = stacktrace;
        }

        public Error(string message, StackTrace stacktrace, Exception innerException) : base(message, innerException) {
            stack = stacktrace;
        }

        // Required default constructors (https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1032):
        public Error() { }
        public Error(string message) : base(message) { }
        public Error(string message, Exception innerException) : base(message, innerException) { }

        public override string StackTrace {
            get {
                if (stack != null) { return stack.ToString(); }
                return base.StackTrace;
            }
        }

    }

    public static class StackTraceExtensions {

        public static Error ToException(this StackTrace self, string message) {
            return new Error(message, self);
        }

    }

    public static class StackTraceV2 {

        public static StackTrace NewStackTrace(int skipFrames = 1) { return new StackTrace(skipFrames, true); }

        public static Error WithAddedOriginalStackTrace<E>(this E self, StackTrace otherStackTrace) where E : Exception {
            return new Error(self.GetType().Name + ": " + self.Message, otherStackTrace, self);
        }

    }

}