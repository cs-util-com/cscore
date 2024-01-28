using com.csutil.io;
using com.csutil.model;
using System.Collections.Generic;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests {
    public class ImageCompareTests {

        public ImageCompareTests(Xunit.Abstractions.ITestOutputHelper logger) { 
        
            logger.UseAsLoggingOutput();

        }

        [Fact]
        public async void TestImageCompare()
        {

            var dir = EnvironmentV2.instance.GetOrAddTempFolder("TestImageComparePictures");

            var imgRef = new FileRef() { url = "https://placekitten.com/1024/512" };
            await imgRef.DownloadTo(dir);
            Log.d("FileRef: " + JsonWriter.AsPrettyString(imgRef));
            Assert.NotNull(imgRef.url);
            Assert.NotNull(imgRef.fileName);
            Assert.NotNull(imgRef.dir);

            FileEntry imgEntry = imgRef.GetFileEntry(dir.FileSystem);
            var img = await ImageLoader.LoadImageInBackground(imgEntry);
            Assert.Equal(1024, img.Width);
            Assert.Equal(512, img.Height);

            byte[] byteArray = new byte[] { 1, 2, 1 };
            Assert.Equal(byteArray, img.Data);

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
