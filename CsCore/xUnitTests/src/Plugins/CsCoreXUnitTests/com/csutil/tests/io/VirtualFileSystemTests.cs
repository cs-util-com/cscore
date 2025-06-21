using System.Linq;
using Xunit;
using Zio;
using Zio.FileSystems;

namespace com.csutil.tests {

    public class VirtualFileSystemTests {

        public VirtualFileSystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            IFileSystem fs = new MemoryFileSystem();
            UPath filePath1 = "/temp.txt";
            string fileContent1 = "This is a content";
            fs.WriteAllText(filePath1, fileContent1);
            Assert.True(fs.FileExists(filePath1));
            Assert.Equal(fileContent1, fs.ReadAllText(filePath1));
            Assert.Single(fs.EnumerateFileSystemEntries(UPath.Root));
        }

        [Fact]
        public void ExampleUsage2() {
            var subDirName1 = "subDir1";
            var fileName1 = "t1.txt";
            var fileName2 = "t2.txt";

            var rootDir = EnvironmentV2.instance.GetRootTempFolder();
            Assert.Null(rootDir.Parent); // not possible to escape the root dir
            Assert.Equal("" + UPath.DirectorySeparator, rootDir.FullName);

            var dirToTestWith = rootDir.GetChildDir("VirtualFileSystemTests.ExampleUsage2");
            dirToTestWith.DeleteV2(); // Delete from prev. tests
            dirToTestWith.GetChild(fileName1).SaveAsText("Test 1");
            dirToTestWith.GetChildDir(subDirName1).GetChild(fileName2).SaveAsText("Test 2");
            Assert.Equal(2, dirToTestWith.EnumerateEntries().Count());

            FileEntry file1 = dirToTestWith.GetChild(fileName1);
            Assert.True(file1.Exists);
            var fileContent1 = file1.LoadAs<string>(null);
            var fileContent2 = file1.LoadAs(typeof(string), null);
            Assert.NotEmpty(fileContent1);
            Assert.Equal(fileContent1, fileContent2);

            DirectoryEntry subDir1 = dirToTestWith.GetChildDir(subDirName1);
            Assert.True(subDir1.Exists);
            Assert.Single(subDir1.EnumerateEntries());

            DirectoryEntry subDir2 = dirToTestWith.GetChildDir("subDir2");
            Assert.False(subDir2.Exists);
            subDir2.Create();
            Assert.True(subDir2.Exists);

            FileEntry self = subDir2.GetChild("t3.txt");
            string text = "Some text 123";
            self.SaveAsText(text);
            Assert.Equal(text, self.LoadAs<string>(null));

            FileEntry f2 = subDir1.GetChild(fileName2);
            Assert.True(f2.Exists);
            subDir1.Delete();
            Assert.False(subDir1.Exists);
            Assert.False(f2.Exists);
        }

    }

}