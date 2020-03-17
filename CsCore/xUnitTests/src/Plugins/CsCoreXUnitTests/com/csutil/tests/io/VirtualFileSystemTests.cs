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
            var dirToTestWith = EnvironmentV2.instance.GetOrAddTempFolder("VirtualFileSystemTests.ExampleUsage2");
            dirToTestWith.DeleteV2(); // Delete from prev. tests
            dirToTestWith.GetChild(fileName1).SaveAsText("Test 1");
            dirToTestWith.GetChildDir(subDirName1).GetChild(fileName2).SaveAsText("Test 2");

            DirectoryEntry rootDir = dirToTestWith;
            Assert.Equal(2, rootDir.EnumerateEntries().Count());
            Assert.Null(rootDir.Parent); // Only possible to operate on the initialy defined root dir

            FileEntry file1 = rootDir.GetChild(fileName1);
            Assert.True(file1.Exists);
            var fileContent1 = file1.LoadAs<string>();
            var fileContent2 = file1.LoadAs(typeof(string));
            Assert.NotEmpty(fileContent1);
            Assert.Equal(fileContent1, fileContent2);

            DirectoryEntry subDir1 = rootDir.GetChildDir(subDirName1);
            Assert.True(subDir1.Exists);
            Assert.Single(subDir1.EnumerateEntries());

            DirectoryEntry subDir2 = rootDir.GetChildDir("subDir2");
            Assert.False(subDir2.Exists);
            subDir2.Create();
            Assert.True(subDir2.Exists);

            FileEntry self = subDir2.GetChild("t3.txt");
            string text = "Some text 123";
            self.SaveAsText(text);
            Assert.Equal(text, self.LoadAs<string>());

            FileEntry f2 = subDir1.GetChild(fileName2);
            Assert.True(f2.Exists);
            subDir1.Delete();
            Assert.False(subDir1.Exists);
            Assert.False(f2.Exists);
        }

    }

}