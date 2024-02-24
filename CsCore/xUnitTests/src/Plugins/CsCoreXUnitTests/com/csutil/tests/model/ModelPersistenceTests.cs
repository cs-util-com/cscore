using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.integrationTests.http;
using com.csutil.io;
using com.csutil.model;
using Xunit;
using Zio;

namespace com.csutil.integrationTests.model {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class ModelPersistenceTests {

        public ModelPersistenceTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            DirectoryEntry dir = EnvironmentV2.instance.GetOrAddTempFolder("Download_ExampleUsage1").CreateV2();
            IFileRef f = new FileRef() { url = "https://raw.githubusercontent.com/cs-util-com/cscore/master/LICENSE" };
            bool wasDownloadNeeded = await f.DownloadTo(dir, (float progress) => {
                Log.d($"Download {progress}% done");
            }, useAutoCachedFileRef: true);
            string licenseText = f.GetFileEntry(dir.FileSystem).LoadAs<string>();
            Assert.Equal(11344, licenseText.Length);
        }

        [Fact]
        public void TestFilePathesInJson() {

            var root = EnvironmentV2.instance.GetOrAddTempFolder("TestFilePathesInJson");
            var file1 = root.GetChildDir("SubDir1").GetChildDir("SubSubDir1").GetChild("child1.txt");
            var savedText = "Test 123";
            file1.SaveAsText(savedText);
            IFileRef x1 = new FileRef();
            x1.SetPath(file1);

            var x2 = x1.DeepCopyViaJson();
            AssertV3.AreEqualJson(x1, x2);
            Assert.NotEmpty(x1.fileName);
            Assert.NotEmpty(x2.fileName);

            // GetChild ensures that no special characters like / are in the file name:
            var fullPathViaGetChild = root.GetChild("" + x2.GetPath());
            Assert.False(fullPathViaGetChild.Exists);

            // ResolveFilePath can be used to resolve full pathes including / characters:
            var file2 = root.ResolveFilePath("" + x2.GetPath());
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
                IFileRef f = new FileRef() { url = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" };
                Assert.True(await TestDownloadTo(f, dir));
                Log.d("FileRef: " + JsonWriter.AsPrettyString(f));
                Assert.False(await TestDownloadTo(f, dir)); // Second time its already cached
            }
            {
                IFileRef f = new FileRef() { url = RestTests.IMG_PLACEHOLD_SERVICE_URL + "/50/50" };
                Assert.True(await TestDownloadTo(f, dir));
                Log.d("FileRef: " + JsonWriter.AsPrettyString(f));
                Assert.True(await TestDownloadTo(f, dir)); // Every time a different image so has to be redownloaded
            }
            {
                IFileRef f = new FileRef() { url = "https://raw.githubusercontent.com/cs-util-com/cscore/master/LICENSE" };
                Assert.True(await TestDownloadTo(f, dir));
                Log.d("FileRef: " + JsonWriter.AsPrettyString(f));
                Assert.False(await TestDownloadTo(f, dir)); // Second time its already cached
            }

        }

        private static async Task<bool> TestDownloadTo(IFileRef f, DirectoryEntry targetDirectory) {
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
            var f = new FileRef() { url = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" };
            await Assert.ThrowsAsync<TaskCanceledException>(async () => {
                await f.DownloadTo(dir, downloadProgress => {
                    if (downloadProgress > 5) { throw new TaskCanceledException("Download canceled after 5%"); }
                }, maxNrOfRetries: 0);
            });
            Assert.NotNull(f.url);
            // Local values should only be set after successful download to have a more atomic predicatable behavior: 
            Assert.Null(f.fileName);
            Assert.Null(f.dir);
            Log.d("FileRef: " + JsonWriter.AsPrettyString(f));

        }

        [Fact]
        public async Task TestImageFileRef() {

            var dir = EnvironmentV2.instance.GetOrAddTempFolder("TestImageFileWithThumbnail");

            var imgRef = new FileRef() { url = RestTests.IMG_PLACEHOLD_SERVICE_URL + "/1024/512" };
            await imgRef.DownloadTo(dir);
            Log.d("FileRef: " + JsonWriter.AsPrettyString(imgRef));
            Assert.NotNull(imgRef.url);
            Assert.NotNull(imgRef.fileName);
            Assert.NotNull(imgRef.dir);

            FileEntry imgEntry = imgRef.GetFileEntry(dir.FileSystem);
            var img = await ImageLoader.LoadImageInBackground(imgEntry);
            Assert.Equal(1024, img.Width);
            Assert.Equal(512, img.Height);

        }

        // [Fact] // the offline test requires help from the developer so its disabled by default 
        public async Task TestOfflineAccessToCachedDownload() {

            var root = EnvironmentV2.instance.GetOrAddTempFolder("TestFileDownload");
            var dir = root.GetChildDir("SubDir1");
            dir.DeleteV2();
            dir.CreateV2();

            // Url from https://gist.github.com/jsturgis/3b19447b304616f18657
            IFileRef f = new FileRef() { url = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" };
            Assert.True(await TestDownloadTo(f, dir));
            Log.d("FileRef: " + JsonWriter.AsPrettyString(f));

            // Wait until the internet is disconnected
            while (InternetStateManager.Instance(this).HasInet) {
                await TaskV2.Delay(1000);
            }

            // Second time its already cached
            Assert.False(await TestDownloadTo(f, dir));
        }

        private class FileRef : IFileRef {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }

    }

}