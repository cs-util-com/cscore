using System;
using System.Collections.Generic;
using System.Text;
using System.Data;



namespace com.csutil.algorithms.images {
    public class MatFloodFill {
        private bool[,] visited;

        public MatFloodFill(int width, int height) {
            visited = new bool[width, height];
        }

        public void FloodFillAlgorithm(Mat<double> image) {
            Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
            int width = image.Cols;
            int height = image.Rows;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (!visited[x, y]) {
                        stack.Push((x, y));

                        while (stack.Count > 0) {
                            var (cx, cy) = stack.Pop();
                            if (cx < 0 || cx >= width || cy < 0 || cy >= height ||
                                SeemsWhite(image, cx, cy) || visited[cx, cy]) {
                                continue;
                            }

                            SetColorAt(image.data, cx, cy, new double[] { 0, 0, 0, 255 }, 4);
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

        private bool SeemsWhite(Mat<double> image, int x, int y) {
            var width = image.Cols;
            var color = GetColorAt(image, x, y);
            return color[0] > 240 && color[1] > 240 && color[2] > 240;
        }

        private static void SetColorAt(double[,] imageData, int x, int y, double[] color, int width) {
            int startIdx = (y * width + x) * 4;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        private static double[] GetColorAt(Mat<double> img, int x, int y) {
            return new double[] { img[x,y], img[x,y+1], img[x,y + 2], img[x,y + 3] };
        }
    }
}
