using com.csutil.io;
using com.csutil.model;
using com.csutil.src.Plugins.CsCore.com.csutil.algorithms;
using StbImageSharp;
using Xunit;
using Zio;
using StbImageWriteSharp;
using System.IO;
using System.Threading.Tasks;

namespace com.csutil.tests.AlgorithmTests {

    public class ImageCompareTests {

        public ImageCompareTests(Xunit.Abstractions.ITestOutputHelper logger) {
            logger.UseAsLoggingOutput();
        }

        [Fact]
        public async Task TestImageCompare() {
            var folderForImageDiffing = EnvironmentV2.instance.GetOrAddTempFolder("TestImageComparePictures");

            MyImageFileRef[] imagesToDownload = {
                new MyImageFileRef {
                    url = "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/diffing%20image%201.png",
                    fileName = "diffing image 1.png"
                },
                new MyImageFileRef {
                    url = "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/diffing%20image%202.jpg",
                    fileName = "diffing image 2.jpg"
                },
            };

            ImageResult[] images = new ImageResult[2];
            for (int i = 0; i < imagesToDownload.Length; i++) {
                await imagesToDownload[i].DownloadTo(folderForImageDiffing, useAutoCachedFileRef: true);
                FileEntry imageFile = imagesToDownload[i].GetFileEntry(folderForImageDiffing.FileSystem);
                images[i] = await ImageLoader.LoadImageInBackground(imageFile);
                Assert.Equal(512, images[i].Width);
                Assert.Equal(512, images[i].Height);
            }

            ImageCompare imageCompareAlgo = new ImageCompare();

            // Compare test for different images -> should result in differences
            ImageCompare.CompareResult imgDifferences1 = imageCompareAlgo.CompareImage(images[0], images[1]);
            Assert.True(imgDifferences1.distortion > 0);
            Assert.Equal(images[0].Width, imgDifferences1.resultImage.Width);
            Assert.Equal(images[0].Height, imgDifferences1.resultImage.Height);

            var imgFileThatHighlightsDifferences = folderForImageDiffing.GetChild("diffingResult1.jpg");
            using (Stream targetStream = imgFileThatHighlightsDifferences.OpenOrCreateForWrite()) {
                imgDifferences1.resultImage.WriteJpgToStream(targetStream, quality: 80);
            }

            // Compare test for the same image -> should result in no differences
            ImageCompare.CompareResult iamgeDifferences2 = imageCompareAlgo.CompareImage(images[0], images[0]);
            Assert.Equal(0, iamgeDifferences2.distortion);

            using (Stream targetStream = folderForImageDiffing.GetChild("diffingResult2.png").OpenOrCreateForWrite()) {
                iamgeDifferences2.resultImage.WritePngToStream(targetStream);
            }

            Log.d($"Stored diffing result images in {folderForImageDiffing.GetFullFileSystemPath()} "
                + $"\n Open diffing file: {imgFileThatHighlightsDifferences.GetFullFileSystemPath()}");
        }

    }

}