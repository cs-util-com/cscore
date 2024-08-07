﻿using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests.images {

    public class FilterTest {

        private const int Radius = 11;
        private const double Eps = (255 * 255) * (0.9 * 0.9);

        [Fact]
        public async Task BoxFilter4ChannelTest() {

            var tempFolder = EnvironmentV2.instance.GetOrAddTempFolder("BoxFilter4ChannelTest");
            var image = await MyImageFileRef.DownloadFileIfNeeded(tempFolder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");
            int boxFilterRadius = 21;
            var boxFilterResult = image.RunBoxFilter(boxFilterRadius);

            var outputFile = tempFolder.GetChild("BoxFilter" + (boxFilterRadius * 2) + ".png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var flippedResult = ImageUtility.FlipImageVertically(boxFilterResult, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, outputStream);

        }

        [Fact]
        public async Task TestColorGuidedFilter() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("ColorGuidedFilterTest");
            var image = await MyImageFileRef.DownloadFileIfNeeded(folder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, (int)image.ColorComponents, Radius, Eps);
            var guidedFilterFinal = guidedFilter.Init(4);
            var colorFiltered = GuidedFilter.GuidedFilterImpl.Filter(image.Data, 4, guidedFilterFinal);

            var colorFilteredFile = folder.GetChild("ColorFiltered.png");
            await using var stream = colorFilteredFile.OpenOrCreateForReadWrite();
            var flippedCf = ImageUtility.FlipImageVertically(colorFiltered, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedCf, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);

        }

        [Fact]
        public async Task TestSingleChannelGuidedFilter() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("SingleChannelGuidedFilterTest");
            var image = await MyImageFileRef.DownloadFileIfNeeded(folder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var guidedFilter = new GuidedFilter(image.Data, image.Width, image.Height, 4, Radius, Eps);

            for (var i = 0; i < (int)image.ColorComponents - 1; i++) {
                var imageSingleChannel = guidedFilter.CreateSingleChannel(image.Data, i);
                var guidedCurrent = new GuidedFilter(imageSingleChannel, image.Width, image.Height, 1, Radius, Eps);
                var guidedMono = guidedCurrent.Init(1);
                var singleGuided = GuidedFilter.GuidedFilterImpl.Filter(imageSingleChannel, 1, guidedMono);

                var singleImage = GuidedFilter.CombineRgb(imageSingleChannel, i);
                var currentFile = folder.GetChild("SingleChannel" + i + ".png");
                {
                    await using var stream = currentFile.OpenOrCreateForReadWrite();
                    var flippedRes = ImageUtility.FlipImageVertically(singleImage, image.Width, image.Height, 3);
                    new ImageWriter().WritePng(flippedRes, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlue, stream);
                }
                var singleFiltered = GuidedFilter.CombineRgb(singleGuided, i);
                currentFile = folder.GetChild("SingleFiltered" + i + ".png");
                {
                    await using var stream = currentFile.OpenOrCreateForReadWrite();
                    var flippedRes = ImageUtility.FlipImageVertically(singleFiltered, image.Width, image.Height, 3);
                    new ImageWriter().WritePng(flippedRes, image.Width, image.Height, ColorComponents.RedGreenBlue, stream);
                }
            }

        }

    }

}