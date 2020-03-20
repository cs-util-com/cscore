using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zio;

namespace com.csutil.tests {

    /// <summary> 
    /// These tests are analog to the FileTests (that work directly with FileInfo and DirectoryInfo) but 
    /// the FileEntry use the new FileSystem abstraction.
    /// </summary>
    public class FileEntryTests {

        public FileEntryTests(Xunit.Abstractions.ITestOutputHelper logger) {
            logger.UseAsLoggingOutput();
            AssertV2.throwExeptionIfAssertionFails = true;
        }

        [Fact]
        public void ExampleUsage1() {

            // Get a directory to work in:
            string dirName = "ExampleDir_" + DateTime.Now.ToUnixTimestampUtc();
            DirectoryEntry myDirectory = EnvironmentV2.instance.GetOrAddTempFolder(dirName);
            Log.d("The directory path is: " + myDirectory.FullName);

            // Get a non-existing child directory
            var childDir = myDirectory.GetChildDir("MyExampleSubDirectory1");

            // Create the sub directory:
            childDir.CreateV2(); // myDirectory.CreateSubdirectory("..") works too

            // Get a file in the child directory:
            FileEntry file1 = childDir.GetChild("MyFile1.txt");

            // Saving and loading from files:
            string someTextToStoreInTheFile = "Some text to store in the file";
            file1.SaveAsText(someTextToStoreInTheFile);
            string loadedText = file1.LoadAs<string>(); // Loading JSON would work as well
            Assert.Equal(someTextToStoreInTheFile, loadedText);

            // Rename the directory:
            var expectedNewPath = childDir.Parent.GetChildDir("MyExampleSubDirectory2").Path;
            Assert.True(childDir.Rename("MyExampleSubDirectory2", out childDir));
            Assert.Equal(expectedNewPath, childDir.Path);
            Assert.Equal("MyExampleSubDirectory2", childDir.Name);
            Assert.Single(childDir.EnumerateFiles());

        }

        [Fact]
        public void TestDelete() {
            // Create a dir with 2 files and a subdir:
            var dir1 = CreateDirectoryForTesting("TestDelete");
            var subDir1 = dir1.GetChildDir("MyExampleSubDirectory1").CreateV2();
            var f1 = dir1.GetChild("MyFile1.txt");
            f1.SaveAsText("Some text 1");
            var f2 = subDir1.GetChild("MyFile2.txt");
            f2.SaveAsText("Some text 2");

            // Now delete the dir that contains the files and the subdir:
            Assert.True(dir1.DeleteV2());
            Assert.False(f1.Exists, "f1=" + f1);
            Assert.False(f2.Exists, "f2=" + f2);
            Assert.False(dir1.Exists, "dir1=" + subDir1);
            Assert.False(subDir1.Exists, "subDir1=" + subDir1);

            Assert.False(subDir1.IsNotNullAndExists(), "IsNotNullAndExists subDir1=" + subDir1);
        }

        [Fact]
        public void TestFolderRename() {

            // Get a directory to work in:
            var myDirectory = CreateDirectoryForTesting("TestFolderRename_" + DateTime.Now.ToUnixTimestampUtc());
            Log.d("The directory path is: " + myDirectory.FullName);

            // Get a non-existing child directory
            var childDir = myDirectory.GetChildDir("MyExampleSubDirectory1");

            // Create the sub directory:
            childDir.CreateV2(); // myDirectory.CreateSubdirectory("..") works too

            // Rename the directory:
            childDir.Rename("MyExampleSubDirectory2", out childDir);
            Assert.Equal("MyExampleSubDirectory2", childDir.Name);
        }

        [Fact]
        public void TestIsNotNullAndExists() {
            DirectoryEntry dir = null;
            Assert.False(dir.IsNotNullAndExists(), "null.IsNotNullAndExists was true");
            Assert.True(EnvironmentV2.instance.GetRootTempFolder().IsNotNullAndExists(),
                "RootTempFolder did not exist:" + EnvironmentV2.instance.GetRootTempFolder());
            Assert.True(EnvironmentV2.instance.GetRootAppDataFolder().IsNotNullAndExists(),
                "RootAppDataFolder did not exist:" + EnvironmentV2.instance.GetRootAppDataFolder());
        }

        [Fact]
        public void TestFileAndDirIteration() {
            AssertV2.throwExeptionIfAssertionFails = true;
            var rootDir = CreateDirectoryForTesting("TestFileAndDirIteration");
            TestIterationInFolder(rootDir);
            TestIterationInFolder(rootDir.GetChildDir("Dir3").CreateV2());
            TestIterationInFolder(rootDir.GetChildDir("Dir3").GetChildDir("Dir3").CreateV2());
        }

        private static void TestIterationInFolder(DirectoryEntry rootDir) {
            rootDir.CreateSubdirectory("Dir1");
            rootDir.GetChildDir("Dir2").CreateV2();
            Assert.True(rootDir.GetChildDir("Dir1").Exists);
            Assert.True(rootDir.GetChildDir("Dir2").IsNotNullAndExists());

            SaveAndLoadTextToFile(rootDir.GetChild("File1.txt"), textToSave: "Test 123");
            Assert.True(rootDir.GetChild("File1.txt").Exists);
            Assert.True(rootDir.GetChild("File1.txt").IsNotNullAndExists());

            Assert.Equal(2, rootDir.GetDirectories().Count());
            Assert.Single(rootDir.GetFiles());
            Assert.Equal(2, rootDir.EnumerateDirectories().Count());
            Assert.Single(rootDir.EnumerateFiles());
            Assert.Equal(3, rootDir.EnumerateEntries().Count());
        }

        [Fact]
        public async Task TestDirectoryMethods() {
            AssertV2.throwExeptionIfAssertionFails = true;
            var rootDir = CreateDirectoryForTesting("DirMethodsTest_" + DateTime.Now.ToUnixTimestampUtc());

            // Test FullPath:
            var dir1 = rootDir.GetChildDir("TestDir 1");
            var alsoDir1 = dir1.Parent.GetChildDir("TestDir 1");
            Assert.Equal(dir1.FullName, alsoDir1.FullName);

            // Test deleting and creating Dir 1:
            dir1.DeleteV2(); // Make sure dir does not yet exist from previous tests
            Assert.False(dir1.IsNotNullAndExists(), "dir1.IsNotNullAndExists");
            Assert.False(dir1.Exists, "dir1.Exists");
            Assert.True(dir1.CreateV2().Exists, "dir1.CreateV2().Exists");
            Assert.True(dir1.IsNotNullAndExists(), "dir1.IsNotNullAndExists");
            dir1.CreateV2(); // Should do nothing and not throw an exception

            var nameOfChildDir1 = "ChildDir 1";
            var nameOfTestFile1InChildDir1 = "Test file 1";
            // Test creating sub dirs and files:
            var subDir = dir1.CreateSubdirectory(nameOfChildDir1);
            SaveAndLoadTextToFile(subDir.GetChild(nameOfTestFile1InChildDir1), textToSave: "Test 123");
            SaveAndLoadTextToFile(subDir.GetChild("Test file 2.txt"), textToSave: "Test 123");

            { // Test moving folders:
                var oldPath = dir1.Parent.GetChildDir(dir1.Name);
                var dir2 = rootDir.GetChildDir("TestDir 2");
                dir2.DeleteV2(); // Make sure dir does not yet exist from previous tests
                await TaskV2.Delay(20);
                Assert.False(dir2.Exists, "before MOVE: dir2.ExistsV2");

                var moveToWorked = dir1.MoveToV2(dir2, out dir1);
                await TaskV2.Delay(100);

                // After move first test that the new dir is now there:
                Assert.True(dir2.Exists, "after MOVE: dir2.ExistsV2");
                Assert.True(dir2.IsNotNullAndExists(), "dafter MOVE: ir2.IsNotNullAndExists");
                Assert.Equal(dir2.FullName, dir1.FullName);
                Assert.True(dir1.Exists, "after MOVE: dir1.ExistsV2");

                Assert.True(moveToWorked, "dir1.MoveToV2(dir2) failed");
                Assert.False(oldPath.Exists, "oldDir2.Exists");

                var subDirs = dir2.GetDirectories();
                Assert.NotEmpty(subDirs);
                var movedChildDir = dir2.GetChildDir(nameOfChildDir1);
                Assert.True(movedChildDir.Exists,
                    "!movedChildDir.ExistsV2, all childDirs=" + subDirs.ToStringV2(sd => "" + sd));
                Assert.NotEmpty(movedChildDir.EnumerateFiles());
                var movedTestFile1 = movedChildDir.GetChild(nameOfTestFile1InChildDir1);
                Assert.True(movedTestFile1.Exists, "movedTestFile1.ExistsV2");
            }
            { // Test that moving to existing folders fails:
                var dir3 = rootDir.GetChildDir("TestDir 3").CreateV2();
                Assert.Equal("TestDir 3", dir3.Name);
                dir3 = rootDir.CreateSubdirectory(dir3.Name);
                AssertV2.Throws<Exception>(() => {
                    dir1.MoveToV2(dir3, out dir1); // This should fail since dir3 already exists
                });
                Assert.NotEqual(dir3.FullName, dir1.FullName);
                dir3.DeleteV2(); // Cleanup after test
            }
            { // Test copying folders:
                var oldPath = dir1.Parent.GetChildDir(dir1.Name);
                var dir4 = rootDir.GetChildDir("TestDir 4");
                dir4.DeleteV2(); // Make sure dir does not yet exist from previous tests
                Assert.False(dir1.IsEmtpy());
                Assert.True(dir1.CopyTo(dir4), "dir1.CopyTo(dir4) failed");
                await TaskV2.Delay(20);
                Assert.True(dir4.IsNotNullAndExists(), "dir4.IsNotNullAndExists");
                Assert.NotEqual(dir4.FullName, dir1.FullName);
                Assert.True(oldPath.Exists, "oldDir4.Exists");

                // Check that the files were really copied from dir1 to dir4:
                var dir4ChildDir1 = dir4.GetChildDir(nameOfChildDir1);
                var testFile1InDir1ChildDir4 = dir4ChildDir1.GetChild(nameOfTestFile1InChildDir1);
                Assert.True(testFile1InDir1ChildDir4.IsNotNullAndExists(), "tf1d4 not found");
                var newTextInTestFile1 = "Some new text for txt file in dir4";
                testFile1InDir1ChildDir4.SaveAsText(newTextInTestFile1);

                // A second copyTo now that dir4 exists should throw an exception:
                Assert.Throws<ArgumentException>(() => { dir1.CopyTo(dir4, replaceExisting: false); });
                // Replacing only works when replaceExisting is set to true:
                Assert.True(dir1.CopyTo(dir4, replaceExisting: true), "dir1.CopyTo(dir4, replaceExisting true) failed");
                // The path to the testFile1 should still exist after copy:
                Assert.True(testFile1InDir1ChildDir4.IsNotNullAndExists(), "old tf1d4 not found");
                // The text should not be newTextInTestFile1 anymore after its replaced by the original again:
                Assert.NotEqual(newTextInTestFile1, testFile1InDir1ChildDir4.LoadAs<string>());

                Assert.True(dir4.DeleteV2(), "dir4.Delete"); // Delete dir4 again
                Assert.False(dir4.DeleteV2(), "dir4.Del"); // dir4 now does not exist anymore so false is returned
            }
            { // Test renaming folders:
                var newName = "TestDir 5";
                rootDir.GetChildDir(newName).DeleteV2();
                var oldPath = dir1.Parent.GetChildDir(dir1.Name);
                Assert.True(dir1.Exists, "dir1.Exists false BEFORE rename");
                Assert.True(dir1.Rename(newName, out dir1), "dir1.Rename(newName) failed");
                Assert.Equal(newName, dir1.Name);
                Assert.True(dir1.Exists, "dir1.Exists false AFTER rename, dir1=" + dir1.FullName);
                if (oldPath.Exists) {
                    var e = Log.e("WebGL renamed via copy but could not delete the original dir=" + oldPath);
                    if (!EnvironmentV2.isWebGL) { throw e; }
                }
            }
            try { rootDir.DeleteV2(); } catch (Exception e) { Log.e("COuld not cleanup the rootDir as the final step", e); }
        }

        private static void SaveAndLoadTextToFile(FileEntry testFile, string textToSave) {
            testFile.SaveAsText(textToSave);
            Assert.True(testFile.IsNotNullAndExists());
            Assert.Equal(textToSave, testFile.LoadAs<string>()); // Load again and compare
        }

        [Fact]
        public void TestFileWriteAndRead() {
            var rootDir = CreateDirectoryForTesting("TestFileWriteAndRead");

            SaveAndLoadTextToFile(rootDir.GetChild("f1.txt"), textToSave: "Test 123");
            Assert.True(rootDir.GetChild("f1.txt").IsNotNullAndExists());

            var objToSave = new MyClass1() { myString = "I am a string", myInt = 123 };
            var jsonFile = rootDir.GetChild("MyClass1InAJsonFile.txt");
            jsonFile.SaveAsJson(objToSave);
            jsonFile.SaveAsJson(objToSave); // This will override the existing file
            MyClass1 loadedObj = jsonFile.LoadAs<MyClass1>(); // Load the object again and compare:
            Assert.Equal(objToSave.myString, loadedObj.myString);
            Assert.Equal(objToSave.myInt, loadedObj.myInt);
            loadedObj = jsonFile.LoadAs<MyClass1>(); // Load the object again and compare:
            Assert.Equal(objToSave.myString, loadedObj.myString);
            Assert.Equal(objToSave.myInt, loadedObj.myInt);
            rootDir.DeleteV2();
        }

        private class MyClass1 {
            public string myString;
            public int myInt;
        }

        private static DirectoryEntry CreateDirectoryForTesting(string name) {
            var rootDir = EnvironmentV2.instance.GetRootTempFolder().GetChildDir(name);
            rootDir.DeleteV2(); // ensure that the test folder does not yet exist
            rootDir.CreateV2();
            Assert.True(rootDir.Exists);
            return rootDir;
        }

        [Fact]
        public void TestFileRenameAndMove() {
            var rootDir = CreateDirectoryForTesting("TestFileRenameAndMove");

            var myFile = rootDir.GetChild("MyFile1.txt");
            SaveAndLoadTextToFile(myFile, textToSave: "Test 123");
            Assert.True(myFile.IsNotNullAndExists());

            var newName = "MyFile2.txt";
            var oldPath = myFile.Parent.GetChild(myFile.Name);
            myFile.Rename(newName, out myFile);
            Assert.Equal(oldPath.Parent.FullName, myFile.Parent.FullName);
            Assert.Equal(newName, myFile.Name);
            Assert.NotEqual(oldPath.Name, myFile.Name);

            var subdir = rootDir.CreateSubdirectory("subdir");
            myFile.MoveToV2(subdir, out myFile);
            Assert.Single(subdir.GetFiles()); // The folder should now contain 1 entry
            Assert.Equal(subdir.FullName, myFile.Parent.FullName);

            rootDir.DeleteV2();
        }

        [Fact]
        public void TestFileWriteAndReadStream() {
            var rootDir = CreateDirectoryForTesting("TestFileWriteAndReadStream");
            var file1 = rootDir.GetChild("f1.txt");
            // Saving the text as a byte stream:
            var text1 = "Test 123";
            using (Stream stream1 = new MemoryStream(Encoding.ASCII.GetBytes(text1))) {
                file1.SaveStream(stream1);
            }
            // Loading the same file again to convert the bytes back into a string:
            using (var stream2 = file1.OpenForRead()) {
                var text2 = Encoding.ASCII.GetString(stream2.ToByteArray());
                Assert.Equal(text1, text2);
            }
            Assert.Throws<FileNotFoundException>(() => {
                rootDir.GetChild("some missing file.txt").OpenForRead();
            });
        }

    }

}