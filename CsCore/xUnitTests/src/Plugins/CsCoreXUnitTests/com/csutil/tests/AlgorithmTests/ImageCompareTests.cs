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
                new FileRef { url = "https://wgrt.com/wp-content/uploads/Cat-Problem-1024x512.png" }, // cat standing up
                new FileRef { url = "https://www.veterinaire.mu/wp-content/uploads/2021/05/cat-1024x512.png" }, // base without line
                //new FileRef { url = "https://i.imgur.com/0FEVxnA.png" } // blue line
                //new FileRef { url = "https://i.imgur.com/6w9dzEk.png" } // brown line
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
            AaaaImageCompareAaaa.CompareResult result = compareObj.ImageCompare(images[0], images[1]);

            Assert.Equal(1024, result.resultImage.Width);
            Assert.Equal(512, result.resultImage.Height);

            result.resultImage.Data = FlipImageHorizontally(result.resultImage.Data, result.resultImage.Width, result.resultImage.Height, result.resultImage.Data.Length);
            Log.d("Distortion: " + result.distortion);

            using (Stream stream = File.OpenWrite(Path.Combine(Path.GetTempPath(), tmpFolderName + "\\result.png"))) {

                ImageWriter writer = new ImageWriter();
                writer.WritePng(result.resultImage.Data, 1024, 512, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                Log.d("Image Out.");

            }

            // Compare test for the same image -> should result in no differences
            AaaaImageCompareAaaa.CompareResult result2 = compareObj.ImageCompare(images[0], images[0]);

            Assert.Equal(1024, result2.resultImage.Width);
            Assert.Equal(512, result2.resultImage.Height);

            result2.resultImage.Data = FlipImageHorizontally(result2.resultImage.Data, result2.resultImage.Width, result2.resultImage.Height, result2.resultImage.Data.Length);
            Log.d("Distortion: " + result2.distortion);

            using (Stream stream = File.OpenWrite(Path.Combine(Path.GetTempPath(), tmpFolderName + "\\result2.png"))) {

                ImageWriter writer = new ImageWriter();
                writer.WritePng(result2.resultImage.Data, 1024, 512, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
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
