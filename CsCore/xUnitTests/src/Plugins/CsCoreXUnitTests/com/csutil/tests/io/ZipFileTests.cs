using System.IO;
using System.Linq;
using com.csutil.io;
using ICSharpCode.SharpZipLib.Zip;
using Xunit;
using Zio;

namespace com.csutil.tests {

    public class ZipFileTests {

        public ZipFileTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            var root = EnvironmentV2.instance.GetOrAddTempFolder("ZipFileTests_ExampleUsage1");
            root.DeleteV2(); // cleanup from previous test if needed

            var dir1 = root.GetChildDir("Dir1");
            var zip1 = root.GetChild("Test1.zip");
            var dir2 = root.GetChildDir("Dir2");

            { // Save a test file in a sub sub dir:
                dir1.GetChildDir("SubDir1").GetChild("t1.txt").SaveAsText("abc");
                dir1.GetChildDir("SubDir2").GetChild("t2.txt").SaveAsText("def");
                var fastZip = new FastZip();
                fastZip.CreateZip(zip1.FullName, dir1.FullName, true, "");
            }
            { // Extract the created zip to dir2:
                var fastZip = new FastZip();
                fastZip.ExtractZip(zip1.FullName, dir2.FullName, null);
            }
            Assert.True(dir2.ExistsV2());
            Assert.Equal("abc", dir2.GetChildDir("SubDir1").GetChild("t1.txt").LoadAs<string>());

            { // Read content directly from zip without extraction:
                var zip = new ZipFile(new FileStream(zip1.FullName, FileMode.Open, FileAccess.Read));
                foreach (ZipEntry zipEntry in zip) { Log.d("entry: " + zipEntry.Name); }
                Assert.Equal(2, zip.Count);
                var e1 = zip.GetEntry("SubDir1/t1.txt");
                Assert.True(e1.IsFile);
                var streamReader = new StreamReader(zip.GetInputStream(e1));
                var fileContent = streamReader.ReadToEnd();
                Assert.Equal("abc", fileContent);
            }

        }

        [Fact]
        public void ExampleUsage2() {

            var root = EnvironmentV2.instance.GetOrAddTempFolder("ZipFileTests_ExampleUsage1");
            root.DeleteV2(); // cleanup from previous test if needed

            var dir1 = root.GetChildDir("Dir1");
            var zip1 = root.GetChild("Test1.zip");

            { // Save a test file in a sub sub dir:
                dir1.GetChildDir("SubDir1").GetChild("t1.txt").SaveAsText("abc");
                dir1.GetChildDir("SubDir2").GetChild("t2.txt").SaveAsText("def");
                dir1.GetChildDir("SubDir2").GetChild("t3.txt").SaveAsText("ghi");
                dir1.GetChildDir("SubDir3").GetChild("t4.txt").SaveAsText("jkl");
                new FastZip().CreateZip(zip1.FullName, dir1.FullName, true, "");
            }

            DirectoryEntry zipContent = OpenZipReadOnly(zip1);
            Assert.Equal(3, zipContent.EnumerateEntries().Count());

            var subDir2 = zipContent.GetChildDir("/SubDir2");
            Assert.Equal(2, subDir2.EnumerateEntries().Count());
            Assert.Equal("def", subDir2.GetChild("t2.txt").LoadAs<string>());

            zipContent.FileSystem.Dispose(); // Close the zip at the end

        }

        private static DirectoryEntry OpenZipReadOnly(FileInfo self) {
            if (self == null) { throw new FileNotFoundException("Passed file was null"); }
            if (!self.ExistsV2()) { throw new FileNotFoundException("Did not find any file at " + self); }
            return OpenZipReadOnly(self.FullName);
        }

        private static DirectoryEntry OpenZipReadOnly(string path) {
            return new ZipFileReadSystem(new ZipFile(path)).GetDirectoryEntry(UPath.Root);
        }

    }

}