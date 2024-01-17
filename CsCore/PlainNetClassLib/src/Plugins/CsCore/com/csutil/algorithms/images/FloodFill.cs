using System;
using System.Drawing;
using ImageMagick;

namespace com.csutil.algorithms.images {
    public class FloodFill {

        private bool[,] visited;
        public FloodFill(int width, int height) {
            visited = new bool[width,height];
        }
        public void FloodFillAlgorithm(byte[] image, int width, int height) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (!visited[x,y]) {
                        FloodFillRecur(image, width, height, x, y);
                    }
                }
            }
        }
        private void FloodFillRecur(byte[] image, int width, int height, int x, int y) {
            if ((x < 0 || x >= width || y < 0 || y >= height)
                || SeemsWhite(image, x, y, width)
                || visited[x,y]  // already visited this square
            ) return;
            SetColorAt(image, x, y, width, new byte[]{0,0,0,255}, 4);
            visited[x,y] = true;  // mark current square as visited
            // recursively call flood fill for neighboring squares
            FloodFillRecur(image, width, height, x, y + 1);
            FloodFillRecur(image, width, height, x, y - 1);
            FloodFillRecur(image, width, height, x - 1, y);
            FloodFillRecur(image, width, height, x + 1, y);

        }

        private bool SeemsWhite(byte[] image, int x, int y, int width) {
            var color = GetColorAt(image, x, y, 4, width);
            return color[0] > 240 && color[1] > 240 && color[2] > 240;
        }
        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            int startIdx = (y * width + x) * bytesPerPixel;
            var colorWithOriginalAlpha = new byte[] { color[0], color[1], color[2], imageData[startIdx + 3] };
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }
        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }   
    }
}
