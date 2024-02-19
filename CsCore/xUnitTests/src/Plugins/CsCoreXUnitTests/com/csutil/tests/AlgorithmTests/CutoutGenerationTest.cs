using System;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {
    public class CutoutGenerationTest {
        

        [Fact]
        public async Task CutoutFromDallE() {
            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("CutoutGeneration");
            var downloadFolder = EnvironmentV2.instance.GetSpecialFolder(Environment.SpecialFolder.UserProfile).GetChildDir("Downloads");
            var dallEImage = await ImageLoader.LoadImageInBackground(downloadFolder.GetChild("unnamed.png"));
            var width = dallEImage.Width;
            var height = dallEImage.Height;
            
            var floodFilled = FloodFill.FloodFillAlgorithm(dallEImage, 240);
            var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, (int)dallEImage.ColorComponents, 30);
            var cutout = GenerateCutOut.Generate(dallEImage, trimap, 10, 1e-5, 129);
            var cutoutPng = folder.GetChild("Cutout.png");
            {
                await using var stream = cutoutPng.OpenOrCreateForReadWrite();
                var writer = new ImageWriter();
                var flipped = ImageUtility.FlipImageVertically(cutout, width, height, (int)dallEImage.ColorComponents);
                writer.WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }
        }
    }
}