using System;
using System.Diagnostics;

namespace com.csutil {

    public class Error : Exception {

        public StackTrace stack;

        public Error(string message, StackTrace stacktrace) : base(message) {
            stack = stacktrace;
        }

        public override string StackTrace => stack.ToString();

    }

}