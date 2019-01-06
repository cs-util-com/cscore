using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil.io.tests {

    public class TestPlayerPrefsV2 {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestFilesWithEnumeratorPasses() {
        }

    }

}
