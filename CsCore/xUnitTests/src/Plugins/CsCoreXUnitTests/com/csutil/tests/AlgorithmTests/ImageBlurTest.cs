using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using Xunit;
using Zio;


namespace com.csutil.tests.AlgorithmTests {
    public class ImageBlurTest {
        public ImageBlurTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task RunBoxBlur_ShouldApplyBlurCorrectly() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("BlurTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            ImageResult image = await ImageLoader.LoadImageInBackground(imageFile);
            byte[] imageResult = ImageBlur.RunBoxBlur(image.Data, image.Width, image.Height, 1, (int)image.ColorComponents);
            ImageResult result = ImageResult.FromMemory(imageResult);

        }
        private static async Task DownloadFileIfNeeded(FileEntry self, string url) {
            var imgFileRef = new MyFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }
        private class MyFileRef : IFileRef {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }
    }        

}
