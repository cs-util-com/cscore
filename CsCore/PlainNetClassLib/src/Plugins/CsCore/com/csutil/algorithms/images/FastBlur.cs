﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Schema;

namespace com.csutil.algorithms.images
{
    public class FastBlur
    {
        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel)
        {
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }



        // Helper method to get color at a given position
        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width)
        {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }

        public static byte[] FastBoxBlur(byte[] img, int width, int height, int radius, int channels)
        {
            var kSize = 2 * radius;
            
            if (kSize % 2 == 0) kSize++;
            var hBlur = new byte[img.Length];
            var avg = (float)1 / kSize;
            Array.Copy(img, hBlur, img.Length);
            for(int j = 0; j < height; j++)
            {
                var hSum = new float[] { 0f, 0f, 0f, 0f, };
                var iAvg = new float[] { 0f, 0f, 0f, 0f, };
                for(int x = 0; x < kSize; x++)
                {
                    var tmpColor = GetColorAt(img, x, j, 4, width);
                    hSum[0] += (float)tmpColor[0];
                    hSum[1] += (float)tmpColor[1];
                    hSum[2] += (float)tmpColor[2];
                    hSum[3] += (float)tmpColor[3];
                }
                iAvg[0] = hSum[0] * avg;
                iAvg[1] = hSum[1] * avg;
                iAvg[2] = hSum[2] * avg;
                iAvg[3] = hSum[3] * avg;
                for(int i = 0; i < width; i++)
                {
                    if(i - kSize/2 >= 0 && i + 1 + kSize/2 < width)
                    {
                        var tmp_pColor = GetColorAt(img, i - kSize / 2, j, 4, width);
                        hSum[0] -= (float)tmp_pColor[0];
                        hSum[1] -= (float)tmp_pColor[1];
                        hSum[2] -= (float)tmp_pColor[2];
                        hSum[3] -= (float)tmp_pColor[3];
                        var tmp_nColor = GetColorAt(img, i + 1 + kSize / 2, j, 4, width);
                        hSum[0] += (float)tmp_nColor[0];
                        hSum[1] += (float)tmp_nColor[1];
                        hSum[2] += (float)tmp_nColor[2];
                        hSum[3] += (float)tmp_nColor[3];
                        //
                        iAvg[0] = hSum[0] * avg;
                        iAvg[1] = hSum[1] * avg;
                        iAvg[2] = hSum[2] * avg;
                        iAvg[3] = hSum[3] * avg;
                    }
                    var bAvg = new byte[iAvg.Length];
                    bAvg[0] = (byte)iAvg[0];
                    bAvg[1] = (byte)iAvg[1];
                    bAvg[2] = (byte)iAvg[2];
                    bAvg[3] = (byte)iAvg[3];
                    SetColorAt(hBlur, i, j, width, bAvg, 4); 

                }
                
            }
            var total = new byte[hBlur.Length];
            Array.Copy(hBlur, total, hBlur.Length);
            for (int i = 0; i < width; i++)
            {
                var tSum = new float[] { 0f, 0f, 0f, 0f };
                var iAvg = new float[] { 0f, 0f, 0f, 0f };
                for (int y = 0; y < kSize; y++)
                {
                    var tmpColor = GetColorAt(hBlur, i, y, 4, width);
                    tSum[0] += (float)tmpColor[0];
                    tSum[1] += (float)tmpColor[1];
                    tSum[2] += (float)tmpColor[2];
                    tSum[3] += (float)tmpColor[3];
                }
                iAvg[0] = tSum[0] * avg;
                iAvg[1] = tSum[1] * avg;
                iAvg[2] = tSum[2] * avg;
                iAvg[3] = tSum[3] * avg;

                for (int j = 0; j < height; j++)
                {
                    if(j - kSize/2 >= 0 && j + 1 + kSize/2 < height)
                    {
                        var tmp_pColor = GetColorAt(hBlur, i, j - kSize / 2, 4, width);
                        tSum[0] -= (float)tmp_pColor[0];
                        tSum[1] -= (float)tmp_pColor[1]; 
                        tSum[2] -= (float)tmp_pColor[2];
                        tSum[3] -= (float)tmp_pColor[3];
                        var tmp_nColor = GetColorAt(hBlur, i, j + 1 + kSize / 2, 4 , width);
                        tSum[0] += (float)tmp_nColor[0];
                        tSum[1] += (float)tmp_nColor[1];
                        tSum[2] += (float)tmp_nColor[2];
                        tSum[3] += (float)tmp_nColor[3];

                        iAvg[0] = tSum[0] * avg;
                        iAvg[1] = tSum[1] * avg;
                        iAvg[2] = tSum[2] * avg;
                        iAvg[3] = tSum[3] * avg;

                    }
                    var bAvg = new byte[iAvg.Length];
                    bAvg[0] = (byte)iAvg[0];
                    bAvg[1] = (byte)iAvg[1];
                    bAvg[2] = (byte)iAvg[2];
                    bAvg[3] = (byte)iAvg[3];
                    SetColorAt(total, i, j, width, bAvg, 4);
                }
            }
            return total;

        }
    }
}
