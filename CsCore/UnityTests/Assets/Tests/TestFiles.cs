using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.files.tests {

    public class TestFiles {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [UnityTest]
        public IEnumerator TestFilesWithEnumeratorPasses() {
            var dir = EnvironmentV2.instance.GetCurrentDirectory();
            Log.d("dir=" + dir.FullPath());
            dir = EnvironmentV2.instance.GetAppDataFolder();
            Log.d("dir=" + dir.FullPath());
            yield return null;
        }

    }

}
