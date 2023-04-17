using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Xunit;
using Zio;

namespace com.csutil.tests {

    public class IoPerformanceTests {

        private const int FILE_COUNT = 10000;

        public IoPerformanceTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void SpeedTestSharpZipLib() {
            var root = EnvironmentV2.instance.GetOrAddTempFolder("SpeedTestSharpZipLib");
            root.DeleteV2(); // cleanup from previous test if needed
            var zipFile = root.CreateV2().GetChild("Test1.zip");
            {
                using var t = Log.MethodEnteredWith($"Write with count {FILE_COUNT}");
                using var zip = new ZipOutputStream(zipFile.OpenOrCreateForWrite());
                zip.SetLevel(0); // 0 means no compression
                for (int i = 0; i < FILE_COUNT; i++) {
                    var entry = new ZipEntry("file" + i);
                    zip.PutNextEntry(entry);
                    WriteAsText(zip, "I am file " + entry.Name);
                    zip.CloseEntry();
                }
            }
            {
                using var t = Log.MethodEnteredWith($"Read with count {FILE_COUNT}");
                using var zip = new ZipFile(zipFile.OpenForRead());
                Assert.Equal(FILE_COUNT, zip.Count);
                foreach (ZipEntry entry in zip) {
                    using var entryStream = new StreamReader(zip.GetInputStream(entry));
                    var fileContent = entryStream.ReadToEnd();
                    Assert.Equal("I am file " + entry.Name, fileContent);
                }
            }
        }

        private static void WriteAsText(ZipOutputStream zip, string fileContent) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
            zip.Write(bytes, 0, bytes.Length);
        }

        [Fact]
        public void SpeedTestZipArchiveFileSystem() {
            var root = EnvironmentV2.instance.GetOrAddTempFolder("SpeedTestZipArchiveFileSystem");
            root.DeleteV2(); // cleanup from previous test if needed
            var zipFile = root.GetChild("Test2.zip");
            root.CreateV2();
            {
                using var t = Log.MethodEnteredWith($"Write with count {FILE_COUNT}");
                using var zipFileSystem = zipFile.OpenAsZip();
                var zip = zipFileSystem.GetRootDirectory();
                for (int i = 0; i < FILE_COUNT; i++) {
                    var entryName = GuidV2.NewGuid() + ".txt";
                    var entry = zip.GetChild(entryName);
                    entry.SaveAsText("I am file " + entryName);
                }
            }
            {
                using var t = Log.MethodEnteredWith($"Read with count {FILE_COUNT}");
                using var zipFileSystem = zipFile.OpenAsZip();
                var zip = zipFileSystem.GetRootDirectory();
                var allFilesInZip = zip.EnumerateEntries().Cast<FileEntry>().ToList();
                Assert.Equal(FILE_COUNT, allFilesInZip.Count());
                foreach (var entry in allFilesInZip) {
                    Assert.Equal("I am file " + entry.Name, entry.LoadAs<string>());
                }
            }
        }

        [Fact]
        public async Task SpeedTestZipArchiveFileSystemAsync() {
            var root = EnvironmentV2.instance.GetOrAddTempFolder("SpeedTestZipArchiveFileSystemAsync");
            root.DeleteV2(); // cleanup from previous test if needed
            var zipFile = root.GetChild("Test3.zip");
            root.CreateV2();
            {
                using var zipFileSystem = zipFile.OpenAsZip();
                await WriteFilesTo(zipFileSystem.GetRootDirectory());
            }
            {
                using var zipFileSystem = zipFile.OpenAsZip();
                ReadAllFilesFrom(zipFileSystem.GetRootDirectory());
            }
        }

        [Fact]
        public async Task SpeedTestFileSystemAsync() {
            var root = EnvironmentV2.instance.GetOrAddTempFolder("SpeedTestFileSystemAsync");
            root.DeleteV2(); // cleanup from previous test if needed
            root.CreateV2();
            await WriteFilesTo(root);
            ReadAllFilesFrom(root);
        }

        [Fact]
        public async Task SpeedTestInMemoryFileSystemAsync() {
            var root = EnvironmentV2.instance.GetNewInMemorySystem();
            await WriteFilesTo(root);
            ReadAllFilesFrom(root);
        }

        private static void ReadAllFilesFrom(DirectoryEntry zip) {
            using var t = Log.MethodEnteredWith($"Read with count {FILE_COUNT}");
            var allFilesInZip = zip.EnumerateEntries().Cast<FileEntry>().ToList();
            Assert.Equal(FILE_COUNT, allFilesInZip.Count());
            foreach (var entry in allFilesInZip) {
                Assert.Equal("I am file " + entry.Name, entry.LoadAs<string>());
            }
        }

        private static async Task WriteFilesTo(DirectoryEntry zip) {
            using var t = Log.MethodEnteredWith($"Write with count {FILE_COUNT}");
            var tasks = new List<Task>();
            for (int i = 0; i < FILE_COUNT; i++) {
                tasks.Add(Task.Run(() => {
                    var entryName = GuidV2.NewGuid() + ".txt";
                    var entry = zip.GetChild(entryName);
                    entry.SaveAsText("I am file " + entryName);
                }));
            }
            await Task.WhenAll(tasks);
        }

    }

}