using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Xunit.Sdk {

    public class EqualException : Exception {

        public EqualException() { }
        protected EqualException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
        public EqualException(string message) : base(message) { }
        public EqualException(string message, Exception innerException) : base(message, innerException) { }

        public EqualException(object expected, object actual) : this($"Expected: {expected} Actual: {actual}") { }

    }

}