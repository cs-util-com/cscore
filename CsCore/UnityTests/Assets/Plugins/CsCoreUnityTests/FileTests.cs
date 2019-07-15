using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.io {

    public class FileTests {

        [SetUp]
        public void BeforeEachTest() { }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestFilesWithEnumeratorPasses() {
            var dir = EnvironmentV2.instance.GetCurrentDirectory();
            Log.d("dir=" + dir.FullPath());
            dir = EnvironmentV2.instance.GetRootAppDataFolder();
            Log.d("dir=" + dir.FullPath());
        }

    }

}
