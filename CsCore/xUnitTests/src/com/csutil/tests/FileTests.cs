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

            {
                var dir1Clone = dir1.Parent.GetChildDir("TestDir 1");
                AssertV2.AreEqual(dir1.FullPath(), dir1Clone.FullPath());
            }

            dir1.DeleteV2();
            Assert.False(dir1.IsNotNullAndExists());
            Assert.True(dir1.CreateV2().Exists);
            Assert.True(dir1.IsNotNullAndExists());
            dir1.Create(); // Should do nothing and not throw an exception
            var subDir = dir1.CreateSubdirectory("ChildDir 1");
            SaveAndLoadTextToFile(subDir.GetChild("Test file 1"));
            SaveAndLoadTextToFile(subDir.GetChild("Test file 2.txt"));

            { // Test moving folders:
                var oldPath = dir1.FullPath();
                var dir2 = rootDir.GetChildDir("TestDir 2");
                dir2.DeleteV2();
                dir1.MoveToV2(dir2);
                Assert.True(dir2.IsNotNullAndExists());
                AssertV2.AreEqual(dir2.FullPath(), dir1.FullPath());
                Assert.False(new DirectoryInfo(oldPath).Exists);
            }
            { // Test that moving to existing folders fails:
                var dir3 = rootDir.GetChildDir("TestDir 3").CreateV2();
                AssertV2.AreEqual("TestDir 3", dir3.Name);
                dir3 = rootDir.CreateSubdirectory(dir3.Name);
                AssertV2.Throws<Exception>(() => {
                    dir1.MoveToV2(dir3); // This should fail since dir3 already exists
                });
                AssertV2.AreNotEqual(dir3.FullPath(), dir1.FullPath());
                dir3.Delete(); // Cleanup after test
            }
            { // Test copying folders:
                var oldPath = dir1.FullPath();
                var dir4 = rootDir.GetChildDir("TestDir 4");
                dir4.DeleteV2(); // Make sure dir does not yet exist from previous tests
                dir1.CopyTo(dir4);
                Assert.True(dir4.IsNotNullAndExists(), "dir=" + dir4.FullPath());
                AssertV2.AreNotEqual(dir4.FullPath(), dir1.FullPath());
                Assert.True(new DirectoryInfo(oldPath).Exists);
                dir4.DeleteV2(); // Cleanup after test
            }
            { // Test renaming folders:
                var newName = "TestDir 5";
                rootDir.GetChildDir(newName).DeleteV2();
                var oldPath = dir1.FullPath();
                dir1.Rename(newName);
                AssertV2.AreEqual(newName, dir1.Name);
                Assert.False(new DirectoryInfo(oldPath).Exists);
            }
            dir1.DeleteV2(); // Cleanup after test
        }

        private static void SaveAndLoadTextToFile(FileInfo testFile, string textToSave = "Test 123") {
            testFile.SaveAsText(textToSave);
            Assert.True(testFile.IsNotNullAndExists());
            AssertV2.AreEqual(textToSave, testFile.LoadAs<string>()); // Load again and compare
        }

        [Fact]
        public void TestFileWriteAndRead() {
            var dir = EnvironmentV2.instance.GetCurrentDirectory().CreateSubdirectory("TestFileWriteAndRead");
            var file1 = dir.GetChild("f1.txt");
            SaveAndLoadTextToFile(file1);
            Assert.True(file1.IsNotNullAndExists());

            var objToSave = new MyClass1() { myString = "I am a string", myInt = 123 };
            file1.SaveAsJson(objToSave); // This will override the existing file
            var loadedObj = file1.LoadAs<MyClass1>(); // Load the object again and compare:
            AssertV2.AreEqual(objToSave.myString, loadedObj.myString);
            AssertV2.AreEqual(objToSave.myInt, loadedObj.myInt);
            dir.DeleteV2();
        }

        [Fact]
        public void TestFileRenameAndMove() {
            var dir = EnvironmentV2.instance.GetCurrentDirectory().CreateSubdirectory("TestFileRename");
            var myFile = dir.GetChild("MyFile1.txt");
            SaveAndLoadTextToFile(myFile);
            Assert.True(myFile.IsNotNullAndExists());

            var newName = "MyFile2.txt";
            var oldPath = new FileInfo(myFile.FullPath());
            myFile.Rename(newName);
            AssertV2.AreEqual(oldPath.ParentDir().FullPath(), myFile.ParentDir().FullPath());
            AssertV2.AreEqual(newName, myFile.Name);
            AssertV2.AreNotEqual(oldPath.Name, myFile.Name);

            var subdir = dir.CreateSubdirectory("subdir");
            myFile.MoveToV2(subdir);
            AssertV2.AreEqual(1, subdir.GetFiles().Count());
            AssertV2.AreEqual(subdir.FullPath(), myFile.ParentDir().FullPath());

            dir.DeleteV2();
        }



        private class MyClass1 {
            public string myString;
            public int myInt;
        }

    }
}