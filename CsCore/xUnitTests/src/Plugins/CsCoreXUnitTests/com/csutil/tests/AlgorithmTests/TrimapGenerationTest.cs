using System;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;
using com.csutil.io;

namespace com.csutil.tests.AlgorithmTests {

    public class TrimapGenerationTest {

        [Fact]
        public async Task TrimapNaive() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FloodFillTesting");
            var downloadFolder = EnvironmentV2.instance.GetSpecialFolder(Environment.SpecialFolder.UserProfile).GetChildDir("Downloads");
            var image = await ImageLoader.LoadImageInBackground(downloadFolder.GetChild("unnamed.png"));

            var width = image.Width;
            var height = image.Height;

            var floodFilled = FloodFill.FloodFillAlgorithm(image, 240);
            {
                var test = folder.GetChild("FloodFilled.png");
                await using var stream = test.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(floodFilled, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            var dilated = Filter.DilateNaive(floodFilled, width, height, 4, 30);
            {
                var dilationPng = folder.GetChild("DilatedNaive.png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(dilated, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, 4, 30);
            {
                var trimapPng = folder.GetChild("TrimapNaive.png");
                await using var stream = trimapPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(trimap, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

        }

        [Fact]
        public async Task TrimapEfficient() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FloodFillTesting");
            var downloadFolder = EnvironmentV2.instance.GetSpecialFolder(Environment.SpecialFolder.UserProfile).GetChildDir("Downloads");
            var image = await ImageLoader.LoadImageInBackground(downloadFolder.GetChild("unnamed.png"));

            var width = image.Width;
            var height = image.Height;
            var kernel = 30;

            var floodFilled = FloodFill.FloodFillAlgorithm(image, 240);
            {
                var test = folder.GetChild("FloodFilled.png");
                await using var stream = test.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(floodFilled, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            var dilated = Filter.Dilate(floodFilled, width, height, 4, kernel);
            var erodedFill = Filter.Erode(floodFilled, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("Eroded.png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(erodedFill, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, 4, kernel);
            {
                var trimapPng = folder.GetChild("Trimap.png");
                await using var stream = trimapPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(trimap, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

        }

    }

}