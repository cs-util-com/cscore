using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.model;
using Xunit;
using Zio;

namespace com.csutil.tests.model {

    public class ModelPersistenceTests {

        public ModelPersistenceTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }


        [Fact]
        public void TestFilePathesInJson() {

            var root = EnvironmentV2.instance.GetOrAddTempFolder("TestFilePathesInJson");
            var file1 = root.GetChildDir("SubDir1").GetChildDir("SubSubDir1").GetChild("child1.txt");
            var savedText = "Test 123";
            file1.SaveAsText(savedText);
            FileRef x1 = new MyFileRef();
            x1.SetPath(file1);

            var x2 = x1.DeepCopyViaJson();
            AssertV2.AreEqualJson(x1, x2);
            Assert.NotEmpty(x1.fileName);
            Assert.NotEmpty(x2.fileName);

            var file2 = root.GetChild("" + x2.GetPath());
            Assert.True(file2.Exists);
            Assert.Equal(savedText, file2.LoadAs<string>());

        }

        [Fact]
        public async Task TestFileDownload() {

            var root = EnvironmentV2.instance.GetOrAddTempFolder("TestFileDownload");
            var dir = root.GetChildDir("SubDir1");
            dir.DeleteV2();
            dir.CreateV2();

            {
                // Url from https://gist.github.com/jsturgis/3b19447b304616f18657
                FileRef f = new MyFileRef() { url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" };
                Assert.True(await TestDownloadTo(f, dir));
                Log.d("FileRef: " + JsonWriter.AsPrettyString(f));
                Assert.False(await TestDownloadTo(f, dir)); // Second time its already cached
            }
            {
                FileRef f = new MyFileRef() { url = "https://picsum.photos/50/50" };
                Assert.True(await TestDownloadTo(f, dir));
                Log.d("FileRef: " + JsonWriter.AsPrettyString(f));
                Assert.True(await TestDownloadTo(f, dir)); // Every time a different image so has to be redownloaded
            }
            {
                FileRef f = new MyFileRef() { url = "https://raw.githubusercontent.com/cs-util-com/cscore/master/LICENSE" };
                Assert.True(await TestDownloadTo(f, dir));
                Log.d("FileRef: " + JsonWriter.AsPrettyString(f));
                Assert.False(await TestDownloadTo(f, dir)); // Second time its already cached
            }

        }

        private static async Task<bool> TestDownloadTo(FileRef f, DirectoryEntry targetDirectory) {
            var t = Log.MethodEntered("Download", f.url, targetDirectory.GetFullFileSystemPath());
            var downloadWasNeeded = await f.DownloadTo(targetDirectory);
            Log.MethodDone(t);
            Assert.NotNull(f.dir);
            Assert.NotNull(f.fileName);
            Assert.NotNull(f.url);
            return downloadWasNeeded;
        }

        [Fact]
        public async Task TestLargeFileDownloadWithProgress() {

            var dir = EnvironmentV2.instance.GetOrAddTempFolder("TestLargeFileDownloadWithProgress");

            // Url from https://gist.github.com/jsturgis/3b19447b304616f18657
            var f = new MyFileRef() { url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" };
            await Assert.ThrowsAsync<TaskCanceledException>(async () => {
                await f.DownloadTo(dir, downloadProgress => {
                    if (downloadProgress > 5) { throw new TaskCanceledException("Download canceled after 5%"); }
                });
            });
            Log.d("FileRef: " + JsonWriter.AsPrettyString(f));

        }

        private class MyFileRef : FileRef {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }

    }

}