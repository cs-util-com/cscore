using NUnit.Framework;
using Zio;

namespace com.csutil.tests.io {

    public class FileTests {

        [SetUp]
        public void BeforeEachTest() { }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestFilesWithEnumeratorPasses() {
            var dir = EnvironmentV2.instance.GetRootTempFolder();
            Log.d("dir=" + dir.FullName);
            Assert.IsNotEmpty(dir.FullName);
            dir = EnvironmentV2.instance.GetOrAddAppDataFolder("MyApp1");
            Log.d("dir=" + dir.FullName);
            Assert.IsNotEmpty(dir.FullName);
        }

    }

}
