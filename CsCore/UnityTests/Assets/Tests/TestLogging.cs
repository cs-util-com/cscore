using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.logging.tests {

    class TestLogging {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestLoggingMethodStartAndEnd() { LoggingExamples.TestLoggingMethodStartAndEnd("I am a string", 123); }

        [Test]
        public void TestBasicLogOutputExamples() { LoggingExamples.TestBasicLogOutputExamples(); }

        [Test]
        public void TestAssertV2Methods() { LoggingExamples.TestAssertV2Methods(); }

        [Test]
        public void TestAssertV2Throws() { LoggingExamples.TestAssertV2Throws(); }

    }

}
