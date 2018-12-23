using System;
using System.Threading;
using com.csutil.logging;
using Xunit;

namespace com.csutil.tests {

    public class LogTests {

        [Fact]
        public void TestLoggingMethodStartAndEnd() { LoggingExamples.TestLoggingMethodStartAndEnd("I am a string", 123); }

        [Fact]
        public void TestBasicLogOutputExamples() { LoggingExamples.TestBasicLogOutputExamples(); }

        [Fact]
        public void TestAssertV2Methods() { LoggingExamples.TestAssertV2Methods(); }

        [Fact]
        public void TestAssertV2Throws() { LoggingExamples.TestAssertV2Throws(); }

    }

}