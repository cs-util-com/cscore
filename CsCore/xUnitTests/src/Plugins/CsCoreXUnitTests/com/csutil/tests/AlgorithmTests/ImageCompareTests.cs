using com.csutil.io;
using com.csutil.model;
using com.csutil.src.Plugins.CsCore.com.csutil.algorithms;
using StbImageSharp;
using System.Collections.Generic;
using Xunit;
using Zio;
using StbImageWriteSharp;
using static com.csutil.http.apis.MetWeatherAPI.Response;
using System.IO;
using System;

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
                new FileRef { url = "https://wgrt.com/wp-content/uploads/Cat-Problem-1024x512.png" },
                new FileRef { url = "https://www.veterinaire.mu/wp-content/uploads/2021/05/cat-1024x512.png" }
            };

            ImageResult[] images = new ImageResult[3];

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
            ImageResult result = compareObj.ImageCompare(images[0], images[1]);

            using (Stream stream = File.OpenWrite("C:\\Users\\Nino\\AppData\\Local\\Temp\\TestImageComparePictures\\result.png")) {

                ImageWriter writer = new ImageWriter();
                writer.WritePng(result.Data, 1024, 512, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                Log.d("Image Out.");

            }

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
