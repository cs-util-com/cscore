using System;
using System.Diagnostics;

namespace com.csutil {

    public class ExceptionWithCustomStack : Exception {

        public StackTrace stack;

        public ExceptionWithCustomStack(string message, StackTrace stacktrace) : base(message) {
            stack = stacktrace;
        }

        public override string StackTrace => stack.ToString();

    }

}