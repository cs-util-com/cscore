using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.csutil.http;
using Xunit;

namespace com.csutil.tests {
    public class FileTests : IDisposable {

        public FileTests() { // Setup before each test
            AssertV2.throwExeptionIfAssertionFails = true;
        }

        public void Dispose() { // TearDown after each test
        }

        [Fact]
        public void TestIsNotNullAndExists() {
            DirectoryInfo appDataFolder = null;
            Assert.False(appDataFolder.IsNotNullAndExists());
            appDataFolder = EnvironmentV2.instance.GetAppDataFolder();
            Log.d("dir=" + appDataFolder.FullPath());
            Assert.True(appDataFolder.IsNotNullAndExists());
        }

        [Fact]
        public void TestFileLoading() {
            var rootDir = EnvironmentV2.instance.GetCurrentDirectory();
            Assert.True(rootDir.IsNotNullAndExists());

            var c1 = rootDir.GetChildDir("testFolder1");
            c1.DeleteV2();
            Assert.False(c1.IsNotNullAndExists());
            Assert.True(c1.CreateV2().Exists);
            Assert.True(c1.IsNotNullAndExists());
            c1.Create(); // should do nothing and not throw an exception
            var testFile = c1.CreateSubdirectory("c1 child 1").GetChild("test");

            var textToWrite = "Test 123";
            testFile.SaveAsJson(textToWrite);
            Assert.True(testFile.IsNotNullAndExists());
            AssertV2.AreEqual(textToWrite, testFile.LoadAs<string>());

            {
                var oldPath = c1.FullPath();
                var c2 = rootDir.GetChildDir("testFolder2");
                c2.DeleteV2();
                c1.MoveToV2(c2);
                Assert.True(c2.IsNotNullAndExists());
                AssertV2.AreEqual(c2.FullPath(), c1.FullPath());
                Assert.False(new DirectoryInfo(oldPath).Exists);
            }
            {
                var c3 = rootDir.GetChildDir("testFolder3").CreateV2();
                AssertV2.Throws<Exception>(() => {
                    c1.MoveToV2(c3); // this should fail since c3 already exists
                });
                AssertV2.AreNotEqual(c3.FullPath(), c1.FullPath());
                c3.Delete();
            }
            {
                var oldPath = c1.FullPath();
                var c4 = rootDir.GetChildDir("testFolder4");
                c4.DeleteV2();
                c1.CopyTo(c4);
                Assert.True(c4.IsNotNullAndExists());
                AssertV2.AreNotEqual(c4.FullPath(), c1.FullPath());
                Assert.True(new DirectoryInfo(oldPath).Exists);
            }
        }

    }
}