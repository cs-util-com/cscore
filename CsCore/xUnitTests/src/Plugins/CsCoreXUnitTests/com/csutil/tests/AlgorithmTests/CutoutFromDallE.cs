using System;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {
    public class CutoutFromDallE {
        

        [Fact]
        public async Task CutoutDallE() {
            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("CutoutGeneration");
            var downloadFolder = EnvironmentV2.instance.GetSpecialFolder(Environment.SpecialFolder.UserProfile).GetChildDir("Downloads");
            var dallEImage = await ImageLoader.LoadImageInBackground(downloadFolder.GetChild("unnamed.png"));
            var width = dallEImage.Width;
            var height = dallEImage.Height;
            
            var floodFilled = FloodFill.FloodFillAlgorithm(dallEImage.Data.DeepCopy(), width, height);
            var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, (int)dallEImage.ColorComponents, 30);
            var cutout = GenerateCutOut.Generate(dallEImage.Data.DeepCopy(), trimap, width, height, (int)dallEImage.ColorComponents);
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