using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.model;
using StbImageWriteSharp;
using Xunit;
using Zio;
using System.Drawing;
using System.Drawing.Imaging;
using com.csutil.http.apis;

namespace com.csutil.tests.AlgorithmTests {
    public class FloodFillTest {
        [Fact]
        public async Task FloodFillDallETest() {

            var downloadFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            var imagePath = Path.Combine(downloadFolderPath, "unnamed.png");
            var image = Image.FromFile(imagePath);
            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FloodFillTesting");
            using (var bitmap = new Bitmap(imagePath))
            {
                // Get image dimensions
                var width = bitmap.Width;
                var height = bitmap.Height;

                // Create byte array to store RGBA data
                var imageData = new byte[width * height * 4];
                // Lock the bits of the image
                BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                // Copy the RGBA data to the byte array
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, imageData, 0, imageData.Length);
                // Unlock the bits
                bitmap.UnlockBits(bmpData);
                var ff = new FloodFill(width, height);
                var floodFilled = ff.FloodFillAlgorithm(imageData, width, height);
                var test = folder.GetChild("FloodFilled.png");
                {
                    using var stream = test.OpenOrCreateForReadWrite();
                    ImageWriter writer = new ImageWriter();
                    writer.WritePng(floodFilled, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                }
                
                var dilated = Filter.Dilate(floodFilled, width, height, 4, 30);
                var dilationPng = folder.GetChild("Dilated.png");
                {
                    using var stream = dilationPng.OpenOrCreateForReadWrite();
                    var writer = new ImageWriter();
                    writer.WritePng(dilated, width, height, ColorComponents.RedGreenBlueAlpha, stream);
                }

                var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, 4, 30);
                var trimapPng = folder.GetChild("Trimap.png");
                {
                    using var stream = trimapPng.OpenOrCreateForReadWrite();
                    var writer = new ImageWriter();
                    writer.WritePng(trimap, width, height, ColorComponents.RedGreenBlueAlpha, stream);
                }
            }
            
        }
        
        
        
        
        private static async Task DownloadFileIfNeeded(FileEntry self, string url)
        {
            var imgFileRef = new MyFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }
        private class MyFileRef : IFileRef
        {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }
    }
}