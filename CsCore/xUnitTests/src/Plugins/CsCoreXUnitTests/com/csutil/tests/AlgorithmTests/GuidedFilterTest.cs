
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

        [Fact]
        public async Task SingleChannelGuidedFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("GuidedTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);

            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, (int)image.ColorComponents, 11, 0.6);
            var imageSingleChannel = guidedFilter.CreateSingleChannel(image.Data, 2);
            var single = folder.GetChild("SingleChannel.png");
            {
                await using var stream = single.OpenOrCreateForWrite();
                var writer = new ImageWriter();
                var flippedSingleChannel = ImageUtility.FlipImageVertically(imageSingleChannel, image.Width, image.Height, (int)image.ColorComponents);
                writer.WritePng(flippedSingleChannel, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            var guidedMono = new GuidedFilter.GuidedFilterMono(imageSingleChannel, image.Width, image.Height, (int)image.ColorComponents, 11, 0.6);
            
            var imageResult = guidedMono.FilterSingleChannel(imageSingleChannel, 1);
            var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("GuidedMono.png");
            {
                await using var stream = test.OpenOrCreateForWrite();
                var writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            var filterTest = GuidedFilter.GuidedFilterImpl.Filter(image.Data, 4, guidedMono);
            var flippedFilterResult = ImageUtility.FlipImageVertically(filterTest, image.Width, image.Height, (int)image.ColorComponents);
            var filter = folder.GetChild("Filter.png");
            {
                await using var stream = filter.OpenOrCreateForWrite();
                var writer = new ImageWriter();
                writer.WritePng(flippedFilterResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
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