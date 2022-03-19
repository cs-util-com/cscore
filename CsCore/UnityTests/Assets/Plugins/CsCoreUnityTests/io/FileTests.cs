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

        [Test]
        public void TestLongPathes01() {
            var dir1 = CreateDirectoryForTesting("TestLongPathes");
            dir1 = dir1.GetChildDir("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            dir1.CreateV2();
            Assert.True(dir1.Exists);
            var file = dir1.GetChild("XXXX.jpg");
            using (var s = file.OpenOrCreateForWrite()) { }
            Assert.True(file.Exists);
        }

        [Test]
        public void TestLongPathes02() {
            var dir1 = CreateDirectoryForTesting("TestLongPathes");
            var subDir = dir1.GetChildDir("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            subDir.CreateV2();
            Assert.True(subDir.Exists);
            var file = subDir.GetChild("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.jpg");
            using (var s = file.OpenOrCreateForWrite()) { }
            Assert.True(file.Exists);
        }

        [Test]
        public void TestLongPathes03() {
            var dir1 = CreateDirectoryForTesting("TestLongPathes");
            var subDir = dir1.GetChildDir("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            subDir.CreateV2();
            Assert.True(subDir.Exists);
            var file = subDir.GetChild("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.jpg");
            using (var s = file.OpenOrCreateForWrite()) { }
            Assert.True(file.Exists);
        }

        [Test]
        public void TestLongPathes04() {
            var dir1 = CreateDirectoryForTesting("TestLongPathes");
            dir1 = dir1.GetChildDir("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            dir1 = dir1.GetChildDir("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            dir1.CreateV2();
            Assert.True(dir1.Exists);
            var file = dir1.GetChild("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.jpg");
            using (var s = file.OpenOrCreateForWrite()) { }
            Assert.True(file.Exists);
        }

        [Test]
        public void TestLongPathes10() {
            var dir1 = CreateDirectoryForTesting("TestLongPathes");
            var subDir = dir1.GetChildDir("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            subDir.CreateV2();
            Assert.True(subDir.Exists);
            var file = subDir.GetChild("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.jpg");
            using (var s = file.OpenOrCreateForWrite()) { }
            Assert.True(file.Exists);
        }

        private static DirectoryEntry CreateDirectoryForTesting(string name) {
            var rootDir = EnvironmentV2.instance.GetRootTempFolder().GetChildDir(name);
            rootDir.DeleteV2(); // ensure that the test folder does not yet exist
            rootDir.CreateV2();
            Assert.True(rootDir.Exists);
            return rootDir;
        }

    }

}
