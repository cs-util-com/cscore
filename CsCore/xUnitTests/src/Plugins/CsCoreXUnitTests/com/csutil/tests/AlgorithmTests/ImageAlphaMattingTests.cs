using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using StbImageWriteSharp;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests {

    public class ImageAlphaMattingTests {

        public ImageAlphaMattingTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary> Ported example usage from https://github.com/atilimcetin/global-matting/tree/master#code  </summary>
        [Fact]
        public async Task TestGlobalMatting() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("ImageMattingTests");
            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
            var trimapFile = folder.GetChild("GT04-trimap.png");
            await DownloadFileIfNeeded(trimapFile, "http://atilimcetin.com/global-matting/GT04-trimap.png");
            var resultOfOriginalCppImplementation = folder.GetChild("GT04-alpha.png");
            await DownloadFileIfNeeded(resultOfOriginalCppImplementation, "http://atilimcetin.com/global-matting/GT04-alpha.png");
            var origAlpha = await ImageLoader.LoadImageInBackground(resultOfOriginalCppImplementation);
            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var trimap = await ImageLoader.LoadImageInBackground(trimapFile);
            var trimapBytes = trimap.Data;
            var imageMatting = new GlobalMatting(image.Data, image.Width, image.Height, (int)image.ColorComponents);
            imageMatting.ExpansionOfKnownRegions(ref trimapBytes, niter: 9);
            imageMatting.RunGlobalMatting(trimapBytes, out var foreground, out var alphaData, out var conf);

            var alphaFol = folder.GetChild("GT04-implementedAlpha.png");
            {
                await using var stream = alphaFol.OpenOrCreateForReadWrite();
                ImageWriter writer = new ImageWriter();
                var flippedResult = ImageUtility.FlipImageVertically(alphaData, image.Width, image.Height, (int)image.ColorComponents);
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            // filter the result with fast guided filter
            var alphaDataGuided = imageMatting.RunGuidedFilter(alphaData, r: 10, eps: 1e-5);

            var alpha = new ImageResult {
                Width = image.Width,
                Height = image.Height,
                SourceComponents = image.ColorComponents,
                ColorComponents = image.ColorComponents,
                BitsPerChannel = image.BitsPerChannel,
                Data = alphaDataGuided
            };

            for (int x = 0; x < trimap.Width; ++x) {
                for (int y = 0; y < trimap.Height; ++y) {
                    if (trimap.GetPixel(x, y).R == 0) {
                        alpha.SetPixel(x, y, new Pixel(0, 0, 0, 0));
                    } else if (trimap.GetPixel(x, y).R == 255) {
                        alpha.SetPixel(x, y, new Pixel(255, 255, 255, 255));
                    }
                }
            }
            // Safe cut out according to alpha region that is 255
            var cutout255 = image.Data;
            var cutout128 = image.Data;
            var cutoutGreater0 = image.Data;

            alpha.Data = alphaData;
            for (var x = 0; x < image.Width; ++x) {
                for (var y = 0; y < image.Height; ++y) {
                    var value = (int)alpha.GetPixel(x, y).A;
                    var idx = (y * image.Width + x) * (int)image.ColorComponents;
                    cutout128[idx + 3] = value >= 128 ? (byte)255 : (byte)0;
                    // cutout255[idx + 3] = value == 255 ? (byte)255 : (byte)0;
                    // cutoutGreater0[idx + 3] = value > 0 ? (byte)255 : (byte)0;
                }
            }
            AssertV3.AreEqual(cutout255, cutoutGreater0);
            var cutout255File = folder.GetChild("Cutout255.png");
            var cutout128File = folder.GetChild("Cutout128.png");
            var cutoutGreater0File = folder.GetChild("cutoutGreater0.png");
            // {
            //     await using var stream = cutout255File.OpenOrCreateForReadWrite();
            //     ImageWriter writer = new ImageWriter();
            //     var im255 = ImageUtility.FlipImageVertically(cutout255, image.Width, image.Height, (int)image.ColorComponents);
            //     writer.WritePng(im255, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            //     
            //     
            // }
            {
                ImageWriter writer = new ImageWriter();
                await using var stream2 = cutout128File.OpenOrCreateForReadWrite();
                var im128 = ImageUtility.FlipImageVertically(cutout128, image.Width, image.Height, (int)image.ColorComponents);
                writer.WritePng(im128, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream2);
            }
            // {
            //     ImageWriter writer = new ImageWriter();
            //     await using var stream3 = cutoutGreater0File.OpenOrCreateForReadWrite();
            //     var im0 = ImageUtility.FlipImageVertically(cutoutGreater0, image.Width, image.Height, (int)image.ColorComponents);
            //     writer.WritePng(im0, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream3);
            // }
        }
        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel)
        {
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }
        [Fact]
        public async Task Testing() {
            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("ImageMattingTests");
            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
            var trimapFile = folder.GetChild("GT04-trimap.png");
            await DownloadFileIfNeeded(trimapFile, "http://atilimcetin.com/global-matting/GT04-trimap.png");
            var resultOfOriginalCppImplementation = folder.GetChild("GT04-alpha.png");
            await DownloadFileIfNeeded(resultOfOriginalCppImplementation, "http://atilimcetin.com/global-matting/GT04-alpha.png");
            var origAlpha = await ImageLoader.LoadImageInBackground(resultOfOriginalCppImplementation);
            var image = await ImageLoader.LoadImageInBackground(imageFile);
            
            // Safe cut out according to alpha region that is 255
            var cutout255 = image.Data;
            var cutout128 = image.Data;
            var cutoutGreater0 = image.Data;

            for (var x = 0; x < image.Width; ++x) {
                for (var y = 0; y < image.Height; ++y) {
                    var value = (int)origAlpha.GetPixel(x, y).R;
                    var idx = (y * image.Width + x) * (int)image.ColorComponents;
                    if(value == 255)
                        SetColorAt(cutout255, x, y, image.Width, new byte[]{255,0,0,255}, 4);
                    // if(value >= 128)
                    //     SetColorAt(cutout128, x, y, image.Width, new byte[]{0,255,0,255}, 4);
                    //
                    //if(value > 0)
                    //    SetColorAt(cutoutGreater0, x, y, image.Width, new byte[]{0,0,0,255}, 4);
                    // cutout255[idx + 3] = value == 255 ? (byte)255 : (byte)0;
                    // cutout128[idx + 3] = value >= 128 ? (byte)255 : (byte)0;
                    // cutoutGreater0[idx + 3] = value > 0 ? (byte)255 : (byte)0;
                }
            }
            var cutout255File = folder.GetChild("Cutout255.png");
            var cutout128File = folder.GetChild("Cutout128.png");
            var cutoutGreater0File = folder.GetChild("cutoutGreater0.png");
            {
                await using var stream = cutout255File.OpenOrCreateForReadWrite();
                ImageWriter writer = new ImageWriter();
                var im255 = ImageUtility.FlipImageVertically(cutout255, image.Width, image.Height, (int)image.ColorComponents);
                writer.WritePng(im255, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                
                
            }
            // {
            //     ImageWriter writer = new ImageWriter();
            //     await using var stream2 = cutout128File.OpenOrCreateForReadWrite();
            //     var im128 = ImageUtility.FlipImageVertically(cutout128, image.Width, image.Height, (int)image.ColorComponents);
            //     writer.WritePng(im128, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream2);
            // }
            // {
            //     ImageWriter writer = new ImageWriter();
            //     await using var stream3 = cutoutGreater0File.OpenOrCreateForReadWrite();
            //     var im0 = ImageUtility.FlipImageVertically(cutoutGreater0, image.Width, image.Height, (int)image.ColorComponents);
            //     writer.WritePng(im0, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream3);
            // }
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

    public struct Pixel {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
        public Pixel(byte r, byte g, byte b, byte a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    public static class ImageResultExtensions {

        public static Pixel GetPixel(this ImageResult self, int x, int y) {
            int index = (y * self.Width + x) * (int)self.ColorComponents;
            if ((int)self.ColorComponents == 3) {
                return new Pixel(self.Data[index], self.Data[index + 1], self.Data[index + 2], 255);
            }
            if ((int)self.ColorComponents == 4) {
                return new Pixel(self.Data[index], self.Data[index + 1], self.Data[index + 2], self.Data[index + 3]);
            }
            throw new Exception("ColorComponents=" + self.ColorComponents);

        }

        public static void SetPixel(this ImageResult self, int x, int y, Pixel p) {
            int index = (y * self.Width + x) * (int)self.ColorComponents;
            if ((int)self.ColorComponents == 3) {
                self.Data[index] = p.R;
                self.Data[index + 1] = p.G;
                self.Data[index + 2] = p.B;
            } else if ((int)self.ColorComponents == 4) {
                self.Data[index] = p.R;
                self.Data[index + 1] = p.G;
                self.Data[index + 2] = p.B;
                self.Data[index + 3] = p.A;
            } else {
                throw new Exception("ColorComponents=" + self.ColorComponents);
            }
        }

    }

}