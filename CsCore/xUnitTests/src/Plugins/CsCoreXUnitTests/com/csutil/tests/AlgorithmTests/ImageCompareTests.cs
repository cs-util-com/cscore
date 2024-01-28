using com.csutil.io;
using com.csutil.model;
using com.csutil.src.Plugins.CsCore.com.csutil.algorithms;
using StbImageSharp;
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

            FileRef[] imgRef = {
                new FileRef { url = "https://placekitten.com/1024/512" },
                new FileRef { url = "https://www.veterinaire.mu/wp-content/uploads/2021/05/cat-1024x512.png" }
            };

            ImageResult[] images = new ImageResult[2];

            for (int i = 0; i < imgRef.Length; i++) {

                await imgRef[i].DownloadTo(dir);
                Log.d("FileRef: " + JsonWriter.AsPrettyString(imgRef));
                Assert.NotNull(imgRef[i].url);
                Assert.NotNull(imgRef[i].fileName);
                Assert.NotNull(imgRef[i].dir);

                FileEntry imgEntry = imgRef[i].GetFileEntry(dir.FileSystem);
                images[i] = await ImageLoader.LoadImageInBackground(imgEntry);

                Assert.Equal(1024, images[i].Width);
                Assert.Equal(512, images[i].Height);

            }
            
            AaaaImageCompareAaaa compareObj = new AaaaImageCompareAaaa();
            Assert.True(compareObj.ImageCompare(images[0], images[0]));
            Assert.False(compareObj.ImageCompare(images[0], images[1]));

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
