using com.csutil.io;
using com.csutil.model;
using com.csutil.src.Plugins.CsCore.com.csutil.algorithms;
using StbImageSharp;
using System.Collections.Generic;
using Xunit;
using Zio;
using StbImageWriteSharp;
using System.IO;

namespace com.csutil.tests.AlgorithmTests {

    public class ImageCompareTests {

        public ImageCompareTests(Xunit.Abstractions.ITestOutputHelper logger) { 
        
            logger.UseAsLoggingOutput();

        }

        [Fact]
        public async void TestImageCompare() {

            string tmpFolderName = "TestImageComparePictures";
            var dir = EnvironmentV2.instance.GetOrAddTempFolder(tmpFolderName);

            FileRef[] imgRef = {
                //new FileRef { url = "https://wgrt.com/wp-content/uploads/Cat-Problem-1024x512.png" },
                new FileRef { url = "https://www.veterinaire.mu/wp-content/uploads/2021/05/cat-1024x512.png" },
                new FileRef { url = "https://i.imgur.com/0FEVxnA.png" }
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

            // Compare test for different images -> should result in differences
            AaaaImageCompareAaaa compareObj = new AaaaImageCompareAaaa();
            ImageResult result = compareObj.ImageCompare(images[0], images[1]);

            Assert.Equal(1024, result.Width);
            Assert.Equal(512, result.Height);

            result.Data = FlipImageHorizontally(result.Data, result.Width, result.Height, result.Data.Length);

            using (Stream stream = File.OpenWrite(Path.Combine(Path.GetTempPath(), tmpFolderName + "\\result.png"))) {

                ImageWriter writer = new ImageWriter();
                writer.WritePng(result.Data, 1024, 512, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                Log.d("Image Out.");

            }

            // Compare test for the same image -> should result in no differences
            ImageResult result2 = compareObj.ImageCompare(images[0], images[0]);

            Assert.Equal(1024, result2.Width);
            Assert.Equal(512, result2.Height);

            result2.Data = FlipImageHorizontally(result2.Data, result2.Width, result2.Height, result2.Data.Length);

            using (Stream stream = File.OpenWrite(Path.Combine(Path.GetTempPath(), tmpFolderName + "\\result2.png"))) {

                ImageWriter writer = new ImageWriter();
                writer.WritePng(result2.Data, 1024, 512, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
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

        private byte[] FlipImageHorizontally(byte[] img, int width, int height, int length) {

            byte[] data = new byte[length];

            for (int i = 0; i < height; i++) {

                for (int j = 0; j < width * 4; j++) {

                    data[(height - 1 - i) * width * 4 + j] = img[i * width * 4 + j];

                }

            }

            return data;

        }

    }

}
