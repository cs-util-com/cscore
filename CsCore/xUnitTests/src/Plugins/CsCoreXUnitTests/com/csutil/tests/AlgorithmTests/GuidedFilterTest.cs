
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using StbImageWriteSharp;
using Xunit;
using Zio;
using ColorComponents = StbImageSharp.ColorComponents;


namespace com.csutil.tests.AlgorithmTests {
    public class GuidedFilterTest {

        private int radius = 11;
        private double eps = (255 * 255) * (0.2 * 0.2);
        
        
        [Fact]
        public async Task SingleChannelGuidedFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("GuidedTesting");
            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
            var image = await ImageLoader.LoadImageInBackground(imageFile);
            
            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, (int)image.ColorComponents, radius, eps);
            for (var i = 0; i < (int)image.ColorComponents - 1; i++) {
                var imageSingleChannel = guidedFilter.CreateSingleChannel(image.Data, i);
                var guidedCurrent = new GuidedFilter(imageSingleChannel, image.Width, image.Height, (int)image.ColorComponents, radius, eps);
                var guidedMono = guidedCurrent.init(1);
                var singleGuided = GuidedFilter.GuidedFilterImpl.Filter(imageSingleChannel, (int)image.ColorComponents, guidedMono);
                var currentFile = folder.GetChild("SingleChanel" + i + ".png");
                {
                    await using var stream = currentFile.OpenOrCreateForReadWrite();
                    var writer = new ImageWriter();
                    var flippedRes = ImageUtility.FlipImageVertically(imageSingleChannel, image.Width, image.Height, (int)image.ColorComponents);
                    writer.WritePng(flippedRes, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                }
                currentFile = folder.GetChild("SingleFiltered" + i + ".png");
                {
                    await using var stream = currentFile.OpenOrCreateForReadWrite();
                    var writer = new ImageWriter();
                    var flippedRes = ImageUtility.FlipImageVertically(singleGuided, image.Width, image.Height, (int)image.ColorComponents);
                    writer.WritePng(flippedRes, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                }
            }
        }
        
        
        
        [Fact]
        public async Task ColorGuidedFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("ColorGuidedTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
            var image = await ImageLoader.LoadImageInBackground(imageFile);

            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, (int)image.ColorComponents, radius, eps);
            var guidedFilterFinal = guidedFilter.init(4);
            var colorFiltered = GuidedFilter.GuidedFilterImpl.Filter(image.Data, 4, guidedFilterFinal);
            var flippedCf = ImageUtility.FlipImageVertically(colorFiltered, image.Width, image.Height, (int)image.ColorComponents);
            var colorFilteredFile = folder.GetChild("ColorFiltered.png");
            {
                await using var stream = colorFilteredFile.OpenOrCreateForReadWrite();
                var writer = new ImageWriter();
                writer.WritePng(flippedCf, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            //var filterTest = GuidedFilter.GuidedFilterImpl.Filter(image.Data, 4, guidedMono);
            //var flippedFilterResult = ImageUtility.FlipImageVertically(filterTest, image.Width, image.Height, (int)image.ColorComponents);
            //var filter = folder.GetChild("Filter.png");
            //{
            //    await using var stream = filter.OpenOrCreateForWrite();
            //    var writer = new ImageWriter();
            //    writer.WritePng(flippedFilterResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            //}
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