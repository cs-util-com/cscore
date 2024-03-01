using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageWriteSharp;
using Xunit;
using Zio;


namespace com.csutil.tests.AlgorithmTests {
    public class GuidedFilterTest {

        private int radius = 11;
        private double eps = (255*255) * (0.9*0.9);
        
        
        [Fact]
        public async Task SingleChannelGuidedFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("GuidedTesting");
            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
            var image = await ImageLoader.LoadImageInBackground(imageFile);
            
            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, 4, radius, eps);
            
            for (var i = 0; i < (int)image.ColorComponents - 1; i++) {
                var imageSingleChannel = guidedFilter.CreateSingleChannel(image.Data, i);
                var guidedCurrent = new GuidedFilter(imageSingleChannel, image.Width, image.Height, 1, radius, eps);
                var guidedMono = guidedCurrent.Init(1);
                var singleGuided = GuidedFilter.GuidedFilterImpl.Filter(imageSingleChannel, 1, guidedMono);

                var singleImage = Combine(imageSingleChannel, i);
                var currentFile = folder.GetChild("SingleChannel" + i + ".png");
                {
                    await using var stream = currentFile.OpenOrCreateForReadWrite();
                    var writer = new ImageWriter();
                    var flippedRes = ImageUtility.FlipImageVertically(singleImage, image.Width, image.Height, 3);
                    writer.WritePng(flippedRes, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlue, stream);
                }
                var singleFiltered = Combine(singleGuided, i);
                currentFile = folder.GetChild("SingleFiltered" + i + ".png");
                {
                    await using var stream = currentFile.OpenOrCreateForReadWrite();
                    var writer = new ImageWriter();
                    var flippedRes = ImageUtility.FlipImageVertically(singleFiltered, image.Width, image.Height, 3);
                    writer.WritePng(flippedRes, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlue, stream);
                }
            }
            
            
        }
        private static byte[] Combine(byte[] singleChannel, int channel) {
            var result = new byte[singleChannel.Length * 3];
            var zeros = new byte[singleChannel.Length ];
            if (channel == 0)
                result = CombineRGB(singleChannel, zeros, zeros, singleChannel.Length);
            else if(channel == 1)
                result = CombineRGB(zeros, singleChannel, zeros, singleChannel.Length);
            else
                result = CombineRGB(zeros, zeros, singleChannel, singleChannel.Length);
            return result;
        }
        private static byte[] CombineRGB(byte[] red, byte[] green, byte[] blue, int length) {
            var result = new byte[length * 3];
            for (int i = 0; i < length * 3; i++) {
                result[i] = (i % 3) switch {
                    0 => red[i / 3],
                    1 => green[i / 3],
                    2 => blue[i / 3],
                    _ => result[i]
                };
            }
            return result;
        }
        
        
        [Fact]
        public async Task ColorGuidedFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("GuidedTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
            var image = await ImageLoader.LoadImageInBackground(imageFile);

            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, (int)image.ColorComponents, radius, eps);
            var guidedFilterFinal = guidedFilter.Init(4);
            var colorFiltered = GuidedFilter.GuidedFilterImpl.Filter(image.Data, 4, guidedFilterFinal);
            var flippedCf = ImageUtility.FlipImageVertically(colorFiltered, image.Width, image.Height, (int)image.ColorComponents);
            var colorFilteredFile = folder.GetChild("ColorFiltered.png");
            {
                await using var stream = colorFilteredFile.OpenOrCreateForReadWrite();
                var writer = new ImageWriter();
                writer.WritePng(flippedCf, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
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