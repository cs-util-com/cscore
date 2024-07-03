using System;
using System.Collections.Generic;
using StbImageSharp;

namespace com.csutil.algorithms.images {

    public static class FloodFill {

        private static readonly byte[] white = new byte[] { 255, 255, 255, 255 };
        private static readonly byte[] black = new byte[] { 0, 0, 0, 255 };

        public static byte[] RunColorCheckAlgorithm(this ImageResult self, int colorThreshold = 240) {
            var image = self.Data.DeepCopy();

            var width = self.Width;
            var height = self.Height;
            var bytesPerPixel = (int)self.ColorComponents;

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    var startIdx = (y * width + x) * bytesPerPixel;
                    var r = image[startIdx];
                    var g = image[startIdx + 1];
                    var b = image[startIdx + 2];
                    var a = image[startIdx + 3];

                    var isBackground = r > colorThreshold && g > colorThreshold && b > colorThreshold;
                    if (isBackground) {
                        SetColorAt(image, x, y, width, black, bytesPerPixel);
                    } else {
                        SetColorAt(image, x, y, width, white, bytesPerPixel);
                    }
                }
            }
            return image;
        }

        public static byte[] RunFloodFill(this ImageResult self, int colorThreshold = 240) {

            var image = self.Data;

            var width = self.Width;
            var height = self.Height;
            var bytesPerPixel = (int)self.ColorComponents;
            var visited = new bool[width, height];

            var result = new byte[self.Data.Length];
            // Fill the entire result array with white:
            for (int i = 0; i < result.Length; i += bytesPerPixel) {
                Array.Copy(white, 0, result, i, bytesPerPixel);
            }

            void FloodFill(int x, int y) {
                var stack = new Stack<(int, int)>();
                stack.Push((x, y));

                while (stack.Count > 0) {
                    var (cx, cy) = stack.Pop();

                    if (cx < 0 || cx >= width || cy < 0 || cy >= height || visited[cx, cy]) {
                        continue;
                    }
                    visited[cx, cy] = true;

                    var startIdx = (cy * width + cx) * bytesPerPixel;
                    var r = image[startIdx];
                    var g = image[startIdx + 1];
                    var b = image[startIdx + 2];

                    var isBackground = r > colorThreshold && g > colorThreshold && b > colorThreshold;
                    if (isBackground) {
                        SetColorAt(result, cx, cy, width, black, bytesPerPixel); // Already black
                        stack.Push((cx + 1, cy));
                        stack.Push((cx - 1, cy));
                        stack.Push((cx, cy + 1));
                        stack.Push((cx, cy - 1));
                    } else {
                        // Its a border pixel, no need to set it to white because the result array is already white
                        // TODO collect border in separate datastructure and return as well?
                    }

                }
            }

            // Start filling from the 4 corners:
            FloodFill(0, 0);
            FloodFill(0, height - 1);
            FloodFill(width - 1, 0);
            FloodFill(width - 1, height - 1);
            return result;
        }

        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            var startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

    }

}