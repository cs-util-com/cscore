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
        return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2] };
    }

    //Potentially optimizable by skipping pixels already in for loop instead of checking throgh if statement 

    public static byte[] RunBoxBlur(byte[] inputImage, int width, int height, int radius, int bytePerPixel) {
        byte[] outputImage = new byte[inputImage.Length];
        int stride = width * bytePerPixel;
        for (int y = radius; y < height - radius; y++) {
            for (int x = radius; x < width - radius; x++) {
                int[] sumColor = new int[bytePerPixel];
                int count = 0;

                for (int dy = -radius; dy <= radius; dy++) {
                    for (int dx = -radius; dx <= radius; dx++) {
                        int positionx = x + dx;
                        int positiony = y + dy;

                        if (positionx >= 0 && positionx < width && positiony >= 0 && positiony < height) {
                            byte[] color = new byte[bytePerPixel];
                            Array.Copy(inputImage, x, color, 0, bytePerPixel);
                            for (int i = 0; i < bytePerPixel; i++) {
                                sumColor[i] += color[i];
                            }
                            count++;
                        }
                    }
                }
                byte[] avgColor = new byte[bytePerPixel];
                for (int i = 0; i < bytePerPixel; i++) {
                    avgColor[i] = (byte)(sumColor[i] / count);
                }

                SetColorAt(outputImage, x, y, width, avgColor, bytePerPixel);

            }
        }
        return outputImage;
    }
}
   
