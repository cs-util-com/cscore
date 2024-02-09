using ImageMagick;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.csutil.algorithms.images {
    public class BoxFilterOnMat {
        public static Mat BoxFilter(Mat img, int radius, int channels) {
            var kSize = 2 * radius;

            if (kSize % 2 == 0) kSize++;
            var hBlur = Mat.Copy(img);
            var avg = (double)1 / kSize;
            var width = img.Cols;
            var height = img.Rows;
            for (int j = 0; j < height; j++) {
                var hSum = new double[] { 0f, 0f, 0f, 0f, };
                var iAvg = new double[] { 0f, 0f, 0f, 0f, };
                for (int x = 0; x < kSize; x++) {
                    var tmpColor = img.GetColor(j, x, channels);
                    hSum[0] += (double)tmpColor[0];
                    hSum[1] += (double)tmpColor[1];
                    hSum[2] += (double)tmpColor[2];
                    hSum[3] += (double)tmpColor[3];
                }
                iAvg[0] = hSum[0] * avg;
                iAvg[1] = hSum[1] * avg;
                iAvg[2] = hSum[2] * avg;
                iAvg[3] = hSum[3] * avg;
                for (int i = 0; i < width; i++) {
                    if (i - kSize / 2 >= 0 && i + 1 + kSize / 2 < width ) {
                        var tmp_pColor = img.GetColor(j, i - kSize / 2, channels);
                        hSum[0] -= (double)tmp_pColor[0];
                        hSum[1] -= (double)tmp_pColor[1];
                        hSum[2] -= (double)tmp_pColor[2];
                        hSum[3] -= (double)tmp_pColor[3];
                        var tmp_nColor = img.GetColor(j, i + 1 + kSize / 2, channels);
                        hSum[0] += (double)tmp_nColor[0];
                        hSum[1] += (double)tmp_nColor[1];
                        hSum[2] += (double)tmp_nColor[2];
                        hSum[3] += (double)tmp_nColor[3];
                        //
                        iAvg[0] = hSum[0] * avg;
                        iAvg[1] = hSum[1] * avg;
                        iAvg[2] = hSum[2] * avg;
                        iAvg[3] = hSum[3] * avg;
                    }
                    var bAvg = new double[iAvg.Length];
                    bAvg[0] = (double)iAvg[0];
                    bAvg[1] = (double)iAvg[1];
                    bAvg[2] = (double)iAvg[2];
                    bAvg[3] = (double)iAvg[3];
                    hBlur.SetColor(j, i, channels, bAvg);

                }

            }
            var total = Mat.Copy(hBlur);
            for (int i = 0; i < width; i++) {
                var tSum = new double[] { 0f, 0f, 0f, 0f };
                var iAvg = new double[] { 0f, 0f, 0f, 0f };
                for (int y = 0; y < kSize; y++) {
                    var tmpColor = hBlur.GetColor(y, i, channels);
                    tSum[0] += (double)tmpColor[0];
                    tSum[1] += (double)tmpColor[1];
                    tSum[2] += (double)tmpColor[2];
                    tSum[3] += (double)tmpColor[3];
                }
                iAvg[0] = tSum[0] * avg;
                iAvg[1] = tSum[1] * avg;
                iAvg[2] = tSum[2] * avg;
                iAvg[3] = tSum[3] * avg;

                for (int j = 0; j < height; j++) {
                    if (j - kSize / 2 >= 0 && j + 1 + kSize / 2 < height) {
                        var tmp_pColor = hBlur.GetColor(j - kSize / 2, i, channels);
                        tSum[0] -= (double)tmp_pColor[0];
                        tSum[1] -= (double)tmp_pColor[1];
                        tSum[2] -= (double)tmp_pColor[2];
                        tSum[3] -= (double)tmp_pColor[3];
                        var tmp_nColor = hBlur.GetColor(j + 1 + kSize / 2, i, channels);
                        tSum[0] += (double)tmp_nColor[0];
                        tSum[1] += (double)tmp_nColor[1];
                        tSum[2] += (double)tmp_nColor[2];
                        tSum[3] += (double)tmp_nColor[3];

                        iAvg[0] = tSum[0] * avg;
                        iAvg[1] = tSum[1] * avg;
                        iAvg[2] = tSum[2] * avg;
                        iAvg[3] = tSum[3] * avg;

                    }
                    var bAvg = new double[iAvg.Length];
                    bAvg[0] = (double)iAvg[0];
                    bAvg[1] = (double)iAvg[1];
                    bAvg[2] = (double)iAvg[2];
                    bAvg[3] = (double)iAvg[3];
                    total.SetColor(j, i, channels, bAvg);
                }
            }
            return total;
        }
    }
}
