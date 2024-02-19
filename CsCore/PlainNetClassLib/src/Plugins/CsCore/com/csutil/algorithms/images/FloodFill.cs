using System;
using System.Collections.Generic;
using StbImageSharp;

namespace com.csutil.algorithms.images {
    public static class FloodFill {
        
        // Flood fill algorithm that floods the image from the border to the inside depending on the pixel value being above the floodValue
        public static  byte[] FloodFillAlgorithm(ImageResult imageRes, int floodValue) {
            var image = imageRes.Data.DeepCopy();
            var width = imageRes.Width;
            var height = imageRes.Height;
            
            var result = new byte[image.Length];
            var visited = new bool[width, height];
            for (var a = 3; a < image.Length; a += 4) {
                result[a] = 255;
            }
            Stack<(int x, int y)> stack = new Stack<(int x, int y)>();

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (!visited[x, y]) {
                        stack.Push((x, y));

                        while (stack.Count > 0) {
                            var (cx, cy) = stack.Pop();
                            if (cx < 0 || cx >= width || cy < 0 || cy >= height ||
                                SeemsValue(image, cx, cy, width, floodValue) || visited[cx, cy]) {
                                continue;
                            }

                            SetColorAt(result, cx, cy, width, new byte[] { 255, 255, 255, 255 }, 4);
                            visited[cx, cy] = true;

                            // Add neighboring pixels to the stack
                            stack.Push((cx, cy + 1));
                            stack.Push((cx, cy - 1));
                            stack.Push((cx - 1, cy));
                            stack.Push((cx + 1, cy));
                        }
                    }
                }
            }
            return result;
        }

        private static bool SeemsValue(byte[] image, int x, int y, int width, int floodValue) {
            var color = GetColorAt(image, x, y, 4, width);
            return color[0] > floodValue && color[1] > floodValue && color[2] > floodValue;
        }

        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            var startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
            var startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }
    }
}