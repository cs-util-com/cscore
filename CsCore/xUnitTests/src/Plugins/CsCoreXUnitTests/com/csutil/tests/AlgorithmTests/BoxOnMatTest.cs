using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests {
    public class BoxOnMatTest {

        [Fact]
        public async Task BoxFilterOnMat4ChannelTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("MatFilterTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");



            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = BoxFilterOnMat.BoxFilter(Mat.ConvertImageToMat(image.Data, image.Width, image.Height, (int)image.ColorComponents), 21, (int)image.ColorComponents);
            var realResult = imageResult.ToByteArray((int)image.ColorComponents);
            var test = folder.GetChild("BoxFilter" + 21 * 2 + ".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(realResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
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
