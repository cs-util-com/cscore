using System;
using System.Collections.Generic;
using System.Text;

public class ImageBlur {

    // Helper method to set the color at a specific location in the image array
    private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
        int startIdx = (y * width + x) * bytesPerPixel;
        Array.Copy(color, 0, imageData, startIdx, color.Length);
    }

    // Helper method to get color at a given position
    private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
        int startIdx = (y * width + x) * bytesPerPixel;
        return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], 255 };
    }

    //Potentially optimizable by skipping pixels already in for loop instead of checking throgh if statement 

    //Todo fix for alpha value to always be the same alpha as the pixel originally had
    public static byte[] RunBoxBlur(byte[] image, int width, int height, int boxSize, int bytePerPixel) {
        var result = new byte[image.Length];

        var halfKernel = (int)Math.Floor((double)boxSize / 2);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var sum = new int[bytePerPixel];
                var count = 0;
                for (var ky = -halfKernel; ky <= halfKernel; ky++)
                {
                    for (var kx = -halfKernel; kx <= halfKernel; kx++)
                    {
                        var offsetX = x + kx;
                        var offsetY = y + ky;

                        if (offsetX < 0 || offsetX >= width || offsetY < 0 || offsetY >= height) continue;
                        
                        var color = GetColorAt(image, offsetX, offsetY, bytePerPixel, width);
                        for (var channel = 0; channel < bytePerPixel; channel++)
                        {
                            sum[channel] += color[channel];
                        }
                        count++;
                    }
                }
                var byteSum = new byte[bytePerPixel];
                for (var channel = 0; channel < bytePerPixel; channel++) {
                    sum[channel] /= count;
                    byteSum[channel] = (byte)sum[channel];
                }
                
                
                SetColorAt(result, x, y, width, byteSum, bytePerPixel);
            }
        }

        return result;
    }
}
   
