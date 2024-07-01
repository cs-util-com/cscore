using System;
using System.Collections.Generic;
using System.Text;
using StbImageSharp;

namespace com.csutil.algorithms.images
{
    public class FloodFillMat
    {
        public static Mat<byte> FloodFillAlgorithm(Mat<byte> image, int floodValue)
        {
            var result = new Mat<byte>(image.Width, image.Height, image.Channels);
            var width = result.Width;
            var height = result.Height; 
            var visited = new bool[width, height];
            result.ColorEntireChannel(4, 255);
            Stack<(int x, int y)> stack = new Stack<(int x, int y)>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y])
                    {
                        stack.Push((x, y));

                        while (stack.Count > 0)
                        {
                            var (cx, cy) = stack.Pop();
                            if (cx < 0 || cx >= width || cy < 0 || cy >= height ||
                                SeemsValue(image, cx, cy, floodValue) || visited[cx, cy])
                            {
                                continue;
                            }

                            result.SetPixel(cx,cy,new byte[] { 255, 255, 255, 255 });
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

        private static bool SeemsValue(Mat<byte> image, int x, int y, int floodValue)
        {
            var color = image.GetPixel(x, y);
            return color[0] > floodValue && color[1] > floodValue && color[2] > floodValue;
        }
    }
}
