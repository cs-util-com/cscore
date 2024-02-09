using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests {
    public class MatClassTest {
        [Fact]
        public void Add_TwoMatrices_ReturnsCorrectSum() {
            // Arrange
            var mat1 = new Mat(2, 2) { [0, 0] = 1, [0, 1] = 2, [1, 0] = 3, [1, 1] = 4 };
            var mat2 = new Mat(2, 2) { [0, 0] = 5, [0, 1] = 6, [1, 0] = 7, [1, 1] = 8 };
            var expected = new Mat(2, 2) { [0, 0] = 6, [0, 1] = 8, [1, 0] = 10, [1, 1] = 12 };

            // Act
            var result = mat1 + mat2;

            // Assert
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    Assert.Equal(expected[i, j], result[i, j]);
                }
            }
        }

        [Fact]
        public void Subtract_TwoMatrices_ReturnsCorrectDifference() {
            // Arrange
            var mat1 = new Mat(2, 2) { [0, 0] = 5, [0, 1] = 6, [1, 0] = 7, [1, 1] = 8 };
            var mat2 = new Mat(2, 2) { [0, 0] = 1, [0, 1] = 2, [1, 0] = 3, [1, 1] = 4 };
            var expected = new Mat(2, 2) { [0, 0] = 4, [0, 1] = 4, [1, 0] = 4, [1, 1] = 4 };

            // Act
            var result = mat1 - mat2;

            // Assert
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    Assert.Equal(expected[i, j], result[i, j]);
                }
            }
        }

        [Fact]
        public void Multiply_TwoMatrices_ReturnsCorrectProduct() {
            // Arrange
            var mat1 = new Mat(2, 3) { [0, 0] = 1, [0, 1] = 2, [0, 2] = 3, [1, 0] = 4, [1, 1] = 5, [1, 2] = 6 };
            var mat2 = new Mat(3, 2) { [0, 0] = 7, [0, 1] = 8, [1, 0] = 9, [1, 1] = 10, [2, 0] = 11, [2, 1] = 12 };
            var expected = new Mat(2, 2) { [0, 0] = 58, [0, 1] = 64, [1, 0] = 139, [1, 1] = 154 };

            // Act
            var result = mat1 * mat2;

            // Assert
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    Assert.Equal(expected[i, j], result[i, j]);
                }
            }
        }

        [Fact]
        public void ElementWiseMultiply_TwoMatrices_ReturnsCorrectProduct() {
            // Arrange
            var mat1 = new Mat(2, 2) { [0, 0] = 1, [0, 1] = 2, [1, 0] = 3, [1, 1] = 4 };
            var mat2 = new Mat(2, 2) { [0, 0] = 5, [0, 1] = 6, [1, 0] = 7, [1, 1] = 8 };
            var expected = new Mat(2, 2) { [0, 0] = 5, [0, 1] = 12, [1, 0] = 21, [1, 1] = 32 };

            // Act
            var result = Mat.ElementWiseMultiply(mat1, mat2);

            // Assert
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    Assert.Equal(expected[i, j], result[i, j]);
                }
            }
        }
        [Fact]
        public async Task CastingWorks() {
            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("MatCastingTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");



            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = Mat.ConvertImageToMat(image.Data, image.Width, image.Height, (int)image.ColorComponents);
            var realResult = imageResult.ToByteArray((int)image.ColorComponents);
            var test = folder.GetChild("Casted Image.png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(realResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
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
