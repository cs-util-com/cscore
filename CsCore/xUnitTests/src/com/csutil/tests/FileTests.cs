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
            DirectoryInfo appDataDir = null;
            Assert.False(appDataDir.IsNotNullAndExists());
            appDataDir = EnvironmentV2.instance.GetAppDataFolder();
            Log.d("appDataDir=" + appDataDir.FullPath());
            Assert.True(appDataDir.IsNotNullAndExists());
        }

        [Fact]
        public void TestDirectoryMethods() {
            var rootDir = EnvironmentV2.instance.GetCurrentDirectory();
            Assert.True(rootDir.IsNotNullAndExists());

            var dir1 = rootDir.GetChildDir("TestDir 1");
            dir1.DeleteV2();
            Assert.False(dir1.IsNotNullAndExists());
            Assert.True(dir1.CreateV2().Exists);
            Assert.True(dir1.IsNotNullAndExists());
            dir1.Create(); // should do nothing and not throw an exception
            var subDir = dir1.CreateSubdirectory("child dir 1");
            SaveAndLoadTextToFile(subDir.GetChild("test file 1"));
            SaveAndLoadTextToFile(subDir.GetChild("test file 2"));

            {
                var oldPath = dir1.FullPath();
                var dir2 = rootDir.GetChildDir("TestDir 2");
                dir2.DeleteV2();
                dir1.MoveToV2(dir2);
                Assert.True(dir2.IsNotNullAndExists());
                AssertV2.AreEqual(dir2.FullPath(), dir1.FullPath());
                Assert.False(new DirectoryInfo(oldPath).Exists);
            }
            { // test that moving to existing folders fails:
                var dir3 = rootDir.GetChildDir("TestDir 3").CreateV2();
                AssertV2.Throws<Exception>(() => {
                    dir1.MoveToV2(dir3); // this should fail since dir3 already exists
                });
                AssertV2.AreNotEqual(dir3.FullPath(), dir1.FullPath());
                dir3.Delete(); // cleanup after test
            }
            {
                var oldPath = dir1.FullPath();
                var dir4 = rootDir.GetChildDir("TestDir 4");
                dir4.DeleteV2(); // make sure dir does not yet exist from previous tests
                dir1.CopyTo(dir4);
                Assert.True(dir4.IsNotNullAndExists());
                AssertV2.AreNotEqual(dir4.FullPath(), dir1.FullPath());
                Assert.True(new DirectoryInfo(oldPath).Exists);
                dir4.DeleteV2(); // cleanup after test
            }
            dir1.DeleteV2(); // cleanup after test
        }

        private static void SaveAndLoadTextToFile(FileInfo testFile, string textToSave = "Test 123") {
            testFile.SaveAsText(textToSave);
            Assert.True(testFile.IsNotNullAndExists());
            AssertV2.AreEqual(textToSave, testFile.LoadAs<string>());
        }

        [Fact]
        public void TestFileWriteAndRead() {
            var dir = EnvironmentV2.instance.GetCurrentDirectory().CreateSubdirectory("TestFileWriteAndRead");
            var file1 = dir.GetChild("f1.txt");
            SaveAndLoadTextToFile(file1);
            Assert.True(file1.IsNotNullAndExists());

            var objToSave = new MyClass1() { s = "I am a string", i = 123 };
            file1.SaveAsJson(objToSave);
            var loadedObj = file1.LoadAs<MyClass1>();
            AssertV2.AreEqual(objToSave.s, loadedObj.s);
            AssertV2.AreEqual(objToSave.i, loadedObj.i);
        }

        private class MyClass1 {
            public string s;
            public int i;
        }

    }
}