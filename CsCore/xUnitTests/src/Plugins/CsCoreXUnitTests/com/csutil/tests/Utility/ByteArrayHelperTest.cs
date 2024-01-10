using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using Xunit;
using Xunit.Abstractions;
using Zio;

namespace com.csutil.tests.AlgorithmTests {

    public class ByteArrayHelperTest {

        private readonly ITestOutputHelper writer;
        public ByteArrayHelperTest(ITestOutputHelper writer) {
            this.writer = writer;
        }

        [Fact]
        public async void  ToBigIntegerToByteTest() {
            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("BlurTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var normal = GuidedFilter.ConvertToDouble(image.Data);
            normal[0] += 0.6;
            writer.WriteLine("Current value is: " + normal[0]);
            writer.WriteLine("Original was: " +  image.Data[0]);
        }
        
       
        [Fact]
        public void BitWiseByteArrayMultTst() {

            var test1 = new byte[] {64,2,3};
            var test2 = new byte[] {5, 6, 7};

            var test3 = GuidedFilter.ByteArrayMult(test1, test2);
        }
        
        
        [Fact]
        public  void BitWiseByteArraySubTst() {

            var test1 = new byte[] {64,2,3};
            var test2 = new byte[] {5, 6, 7};

            var test3 = GuidedFilter.ByteArraySub(test1, test2);
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