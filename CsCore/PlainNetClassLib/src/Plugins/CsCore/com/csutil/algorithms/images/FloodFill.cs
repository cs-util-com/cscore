using System;
using System.Collections.Generic;

namespace com.csutil.algorithms.images {
    public class FloodFill {

        private bool[,] visited;

        public FloodFill(int width, int height) {
            visited = new bool[width, height];
        }

        public void FloodFillAlgorithm(byte[] image, int width, int height) {
            Stack<(int x, int y)> stack = new Stack<(int x, int y)>();

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (!visited[x, y]) {
                        stack.Push((x, y));

                        while (stack.Count > 0) {
                            var (cx, cy) = stack.Pop();
                            if (cx < 0 || cx >= width || cy < 0 || cy >= height ||
                                SeemsWhite(image, cx, cy, width) || visited[cx, cy]) {
                                continue;
                            }

                            SetColorAt(image, cx, cy, width, new byte[] { 0, 0, 0, 255 }, 4);
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
        }

        private bool SeemsWhite(byte[] image, int x, int y, int width) {
            var color = GetColorAt(image, x, y, 4, width);
            return color[0] > 240 && color[1] > 240 && color[2] > 240;
        }

        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }
    }
}