using System;

namespace com.csutil {
    public class InjectionException : Exception {
        public InjectionException(string message) : base(message) { }
    }
}