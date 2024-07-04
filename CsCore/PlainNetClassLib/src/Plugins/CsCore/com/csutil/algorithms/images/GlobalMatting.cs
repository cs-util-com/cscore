using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.math;

namespace com.csutil.algorithms.images {

    /// <summary> Ported version of https://raw.githubusercontent.com/atilimcetin/global-matting/master/globalmatting.cpp </summary>
    public class GlobalMatting {

        private byte[] black = new byte[] { 0, 0, 0, 255 };
        private byte[] white = new byte[] { 255, 255, 255, 255 };

        Random rand = new Random();
        
        private byte[] image;
        private int width;
        private int height;
        private int bytesPerPixel;

        public GlobalMatting(byte[] image, int width, int height, int bytesPerPixel) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;
        }

        // Example of converting a function to calculate the squared value
        private static float Sqr(float a) {
            return a * a;
        }

        // Assuming a Point struct similar to cv::Point
        public struct Point {
            public int X, Y;

            public Point(int x, int y) {
                X = x;
                Y = y;
            }
        }

        public struct PixelRgb {
            public byte R, G, B;

            public PixelRgb(byte r, byte g, byte b) {
                R = r;
                G = g;
                B = b;
            }
        }

        // Method to find boundary pixels
        public List<Point> FindBoundaryPixels(byte[] trimap, byte a, byte b) {
            List<Point> result = new List<Point>();

            for (int x = 1; x < width - 1; ++x) {
                for (int y = 1; y < height - 1; ++y) {
                    int idx = (y * width + x) * bytesPerPixel; // Calculate the index for the current pixel

                    // Check the current pixel's value and its neighbors to find boundary pixels
                    if (trimap[idx] == a) {
                        if (trimap[idx - width] == b || // pixel above
                            trimap[idx + width] == b || // pixel below
                            trimap[idx - bytesPerPixel] == b || // pixel to the left
                            trimap[idx + bytesPerPixel] == b) // pixel to the right
                        {
                            result.Add(new Point(x, y));
                        }
                    }
                }
            }

            return result;
        }

        // Calculate alpha value (Eq. 2 in the C++ code)
        private float CalculateAlpha(PixelRgb F, PixelRgb B, PixelRgb I) {
            float result = 0;
            float div = 1e-6f;
            result += (I.R - B.R) * (F.R - B.R);
            div += (F.R - B.R) * (F.R - B.R);
            result += (I.G - B.G) * (F.G - B.G);
            div += (F.G - B.G) * (F.G - B.G);
            result += (I.B - B.B) * (F.B - B.B);
            div += (F.B - B.B) * (F.B - B.B);
            return Math.Min(Math.Max(result / div, 0.0f), 1.0f);
        }

        // Color cost (Eq. 3 in the C++ code)
        private float ColorCost(PixelRgb F, PixelRgb B, PixelRgb I, float alpha) {
            float result = 0;
            result += Sqr(I.R - (alpha * F.R + (1 - alpha) * B.R));
            result += Sqr(I.G - (alpha * F.G + (1 - alpha) * B.G));
            result += Sqr(I.B - (alpha * F.B + (1 - alpha) * B.B));
            return (float)Math.Sqrt(result);
        }

        // Equation 4: Distance cost between two points
        public static float DistCost(Point p0, Point p1, float minDist) {
            var dist = Sqr(p0.X - p1.X) + Sqr(p0.Y - p1.Y);
            return (float)Math.Sqrt(dist) / minDist;
        }

        // Color distance between two pixels
        public static float ColorDist(PixelRgb I0, PixelRgb I1) {
            float result = 0;

            result += Sqr(I0.R - I1.R);
            result += Sqr(I0.G - I1.G);
            result += Sqr(I0.B - I1.B);

            return (float)Math.Sqrt(result);
        }

        // Nearest distance from a point to any point in a boundary
        public static float NearestDistance(List<Point> boundary, Point p) {
            float minDist2 = float.MaxValue;
            foreach (var bp in boundary) {
                float dist2 = Sqr(bp.X - p.X) + Sqr(bp.Y - p.Y);
                minDist2 = Math.Min(minDist2, dist2);
            }

            return (float)Math.Sqrt(minDist2);
        }

        // Comparison delegate for sorting by intensity
        public int CompareIntensity(Point p0, Point p1) {
            var c0 = GetColorAt(image, p0.X, p0.Y);
            var c1 = GetColorAt(image, p1.X, p1.Y);

            int intensity0 = c0.R + c0.G + c0.B;
            int intensity1 = c1.R + c1.G + c1.B;

            return intensity0.CompareTo(intensity1);
        }

        private byte[] colorCache = new byte[4];

        // Helper method to get color at a given position
        private PixelRgb GetColorAt(byte[] img, int x, int y) {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new PixelRgb(img[startIdx], img[startIdx + 1], img[startIdx + 2]);
        }

        // Method for expansion of known regions
        public void ExpansionOfKnownRegions(ref byte[] trimap, int r, float c) {
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    int idx = (y * width + x) * bytesPerPixel;

                    if (trimap[idx] != 128) // Assuming 128 represents the unknown region
                        continue;

                    var I = GetColorAt(image, x, y);

                    for (int j = y - r; j <= y + r; ++j) {
                        for (int i = x - r; i <= x + r; ++i) {
                            if (i < 0 || i >= width || j < 0 || j >= height)
                                continue;

                            int neighborIdx = (j * width + i) * bytesPerPixel;

                            if (trimap[neighborIdx] != 0 && trimap[neighborIdx] != 255)
                                continue;

                            var I2 = GetColorAt(image, i, j);

                            float pd = (float)Math.Sqrt(Sqr(x - i) + Sqr(y - j));
                            float cd = ColorDist(I, I2);

                            if (pd <= r && cd <= c) {
                                if (trimap[neighborIdx] == 0)
                                    trimap[idx] = 1;
                                else if (trimap[neighborIdx] == 255)
                                    trimap[idx] = 254;
                            }
                        }
                    }
                }
            }

            // Update the trimap values
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    int idx = (y * width + x) * bytesPerPixel;

                    if (trimap[idx] == 1)
                        trimap[idx] = 0;
                    else if (trimap[idx] == 254)
                        trimap[idx] = 255;
                }
            }
        }

        // Method to expand known regions in the trimap with checks
        public void ExpansionOfKnownRegions(ref byte[] trimap, int niter) {
            // Check if the image or trimap is empty
            if (image == null || image.Length == 0)
                throw new ArgumentException("image is empty");
            if (trimap == null || trimap.Length == 0)
                throw new ArgumentException("trimap is empty");

            // Assuming bytesPerPixel is the number of channels (e.g., 3 for an RGB image)
            if (image.Length != width * height * bytesPerPixel)
                throw new ArgumentException("image must have CV_8UC3 type (3 channels) but was bytesPerPixel=" + bytesPerPixel);

            // Loop through iterations and call helper functions
            for (int i = 0; i < niter; ++i) {
                // Calculate the scaling factor for the radius and color difference threshold
                // The original C++ code seems to use 'niter - i' for the threshold, which decreases with each iteration.
                // It is not clear what the intended effect is, so you might need to adjust this based on the expected behavior.
                float scalingFactor = (float)(niter - i) / niter;
                ExpansionOfKnownRegionsHelper(image, trimap, i + 1, scalingFactor);
            }
            ErodeFB(ref trimap, 2);
        }

        // This method performs an expansion of known regions
        public void ExpansionOfKnownRegionsHelper(byte[] image, byte[] trimap, int r, float c) {
            int w = width;
            int h = height;
            // ... (Conversion logic) ...
            // Loop through pixels and perform calculations based on the C++ logic
            // ... 
            for (var x = 0; x < w; ++x) {
                for (var y = 0; y < h; ++y) {
                    if (!ColorIsValue(trimap, x, y, 128))
                        continue;
                    var im = GetColorAt(image, x, y);

                    for (var j = y - r; j <= y + r; ++j) {
                        for (var i = x - r; i <= x + r; ++i) {
                            if (i < 0 || i >= w || j < 0 || j >= h)
                                continue;
                            if (!ColorIsValue(trimap, i, j, 0) && !ColorIsValue(trimap, i, j, 255))
                                continue;

                            var imCur = GetColorAt(image, i, j);
                            var pd = (float)Math.Sqrt(Sqr(x - i) + Sqr(y - j));
                            var cd = ColorDist(im, imCur);

                            if (!(pd <= r) || !(cd <= c))
                                continue;

                            if (ColorIsValue(trimap, i, j, 0)) {
                                colorCache[0] = 1;
                                colorCache[1] = 1;
                                colorCache[2] = 1;
                                colorCache[3] = 255;
                                SetColorAt(trimap, x, y, colorCache);
                                // SetColorAt(trimap, x, y, new byte[] { 1, 1, 1, 255 });
                            } else if (ColorIsValue(trimap, i, j, 255)) {
                                colorCache[0] = 254;
                                colorCache[1] = 254;
                                colorCache[2] = 254;
                                colorCache[3] = 255;
                                SetColorAt(trimap, x, y, colorCache);
                                // SetColorAt(trimap, x, y, new byte[] { 254, 254, 254, 255 });
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    if (ColorIsValue(trimap, x, y, 1))
                        SetColorAt(trimap, x, y, black);
                    else if (ColorIsValue(trimap, x, y, 254))
                        SetColorAt(trimap, x, y, white);
                }
            }
        }

        // Method to perform erosion on foreground and background regions
        private void ErodeFB(ref byte[] trimap, int r) {
            int w = width;
            int h = height;
            var size = w * h * bytesPerPixel;
            byte[] foreground = new byte[size];
            byte[] background = new byte[size];

            // Initialize foreground and background maps
            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    if (ColorIsValue(trimap, x, y, 0))
                        SetColorAt(background, x, y, new byte[] { 1, 1, 1, 255 });
                    else if (ColorIsValue(trimap, x, y, 255))
                        SetColorAt(foreground, x, y, new byte[] { 1, 1, 1, 255 });
                }
            }

            // Erode the foreground and background
            byte[] erodedBackground = Filter.Erode(background, w, h, 4, r);
            byte[] erodedForeground = Filter.Erode(foreground, w, h, 4, r);

            // Increase unknown region
            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    if (ColorIsValue(erodedBackground, x, y, 0) && ColorIsValue(erodedForeground, x, y, 0))
                        SetColorAt(trimap, x, y, new byte[] { 128, 128, 128, 255 }); // Set to unknown
                }
            }
        }

        // Checks for up to 3 size length color array if the color is equal to the input value
        private bool ColorIsValue(byte[] array, int x, int y, int value) {
            var color = GetColorAt(array, x, y);
            return color.R == value && color.G == value && color.B == value;
        }

        // Helper method to erode an image
        private byte[] Erode(byte[] image, int w, int h, int r) {
            byte[] erodedImage = new byte[image.Length];
            Array.Copy(image, erodedImage, image.Length);

            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    bool erodePixel = false;
                    for (int dy = -r; dy <= r; ++dy) {
                        for (int dx = -r; dx <= r; ++dx) {
                            if (dx * dx + dy * dy <= r * r) {
                                int nx = x + dx;
                                int ny = y + dy;
                                if (nx >= 0 && nx < w && ny >= 0 && ny < h) {
                                    int idx = (ny * w + nx) * bytesPerPixel;
                                    // If any pixel in the neighborhood is 0, erode the current pixel
                                    if (image[idx] == 0) {
                                        erodePixel = true;
                                    }
                                }
                            }
                        }
                    }

                    if (erodePixel) {
                        int idx = (y * w + x) * bytesPerPixel;
                        erodedImage[idx] = 0;
                    }
                }
            }

            return erodedImage;
        }

        public struct Sample {
            public int fi, bj;
            public float df, db;
            public float cost, alpha;
        }

        // Method to calculate alpha patch match
        public void CalculateAlphaPatchMatch(byte[] trimap, List<Point> foregroundBoundary, List<Point> backgroundBoundary, out Sample[][] samples) {
            int w = width;
            int h = height;

            samples = new Sample[h][];
            for (int i = 0; i < h; i++)
                samples[i] = new Sample[w];

            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    if (ColorIsValue(trimap, x, y, 128)) {
                        Point p = new Point(x, y);

                        samples[y][x].fi = rand.Next(foregroundBoundary.Count);
                        samples[y][x].bj = rand.Next(backgroundBoundary.Count);
                        samples[y][x].df = NearestDistance(foregroundBoundary, p);
                        samples[y][x].db = NearestDistance(backgroundBoundary, p);
                        samples[y][x].cost = float.MaxValue;
                    }
                }
            }

            // Create and shuffle coordinates
            List<Point> coords1 = new List<Point>();
            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    coords1.Add(new Point(x, y));
            var coords2 = rand.ShuffleEntries(coords1);

            // Propagation
            for (int iter = 0; iter < 10; ++iter) {
                foreach (Point p in coords2) {
                    int x = p.X;
                    int y = p.Y;

                    if (!ColorIsValue(trimap, x, y, 128)) { continue; }


                    var I = GetColorAt(image, x, y);

                    Sample s = samples[y][x];

                    // Propagation

                    // Propagation: Check the neighbors' samples
                    for (int y2 = y - 1; y2 <= y + 1; ++y2) {
                        for (int x2 = x - bytesPerPixel; x2 <= x + bytesPerPixel; x2 += bytesPerPixel) {
                            if (x2 < 0 || x2 >= w || y2 < 0 || y2 >= h)
                                continue;

                            if (!ColorIsValue(trimap, x2, y2, 128))
                                continue;

                            Sample s2 = samples[y2][x2];

                            // ... Calculate cost ...
                            Point fp = foregroundBoundary[s2.fi];
                            Point bp = backgroundBoundary[s2.bj];
                            var F = GetColorAt(image, fp.X, fp.Y);
                            var B = GetColorAt(image, bp.X, bp.Y);
                            float alpha = CalculateAlpha(F, B, I);
                            float cost = ColorCost(F, B, I, alpha) + DistCost(p, fp, s.df) + DistCost(p, bp, s.db);

                            // If new cost is lower, update the sample
                            if (cost < s.cost) {
                                s.fi = s2.fi;
                                s.bj = s2.bj;
                                s.cost = cost;
                                s.alpha = alpha;
                            }
                        }
                    }

                    // Random walk
                    int w2 = Math.Max(foregroundBoundary.Count, backgroundBoundary.Count);

                    for (int k = 0;; k++) {
                        float r = w2 * (float)Math.Pow(0.5, k);

                        if (r < 1) { break; }


                        int di = (int)(r * rand.NextFloat());
                        int dj = (int)(r * rand.NextFloat());

                        int fi = s.fi + (rand.NextFloat() > 0.5 ? di : -di);
                        int bj = s.bj + (rand.NextFloat() > 0.5 ? dj : -dj);

                        if (fi < 0 || fi >= foregroundBoundary.Count || bj < 0 || bj >= backgroundBoundary.Count) { continue; }


                        Point fp = foregroundBoundary[fi];
                        Point bp = backgroundBoundary[bj];

                        var F = GetColorAt(image, fp.X, fp.Y);
                        var B = GetColorAt(image, bp.X, bp.Y);

                        float alpha = CalculateAlpha(F, B, I);
                        float cost = ColorCost(F, B, I, alpha) + DistCost(p, fp, s.df) + DistCost(p, bp, s.db);

                        // If new cost is lower, update the sample
                        if (cost < s.cost) {
                            s.fi = fi;
                            s.bj = bj;
                            s.cost = cost;
                            s.alpha = alpha;
                        }
                    }

                    // After propagation and random walk, assign the sample back
                    samples[y][x] = s;

                }
            }

        }

        public void GlobalMattingHelper(byte[] trimap, out byte[] foreground, out byte[] alpha, out byte[] conf) {
            List<Point> foregroundBoundary = FindBoundaryPixels(trimap, 255, 128);
            List<Point> backgroundBoundary = FindBoundaryPixels(trimap, 0, 128);

            int n = foregroundBoundary.Count + backgroundBoundary.Count;
            for (int i = 0; i < n; ++i) {
                int x = rand.Next(width);
                int y = rand.Next(height);

                if (ColorIsValue(trimap, x, y, 0))
                    backgroundBoundary.Add(new Point(x, y));
                else if (ColorIsValue(trimap, x, y, 255))
                    foregroundBoundary.Add(new Point(x, y));
            }

            foregroundBoundary.Sort((p0, p1) => IntensityComp(p0, p1));
            backgroundBoundary.Sort((p0, p1) => IntensityComp(p0, p1));

            CalculateAlphaPatchMatch(trimap, foregroundBoundary, backgroundBoundary, out var samples);

            // Initialize output arrays
            foreground = new byte[width * height * bytesPerPixel];
            alpha = new byte[width * height * bytesPerPixel];
            conf = new byte[width * height * bytesPerPixel];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int idx = (y * width + x) * bytesPerPixel;
                    switch (trimap[idx]) {
                        case 0:
                            SetColorAt(alpha, x, y, black);
                            conf[idx] = 255;
                            SetColorAt(foreground, x, y, black);
                            break;
                        case 128:
                            Sample s = samples[y][x];
                            var alphaValue = (byte)(255 * s.alpha);
                            SetColorAt(alpha, x, y, new byte[] { alphaValue, alphaValue, alphaValue, 255 });
                            conf[idx] = (byte)(255 * Math.Exp(-s.cost / 6));
                            Point p = foregroundBoundary[s.fi];
                            var color = GetColorAt(image, p.X, p.Y);
                            SetColorAt(foreground, x, y, color);
                            break;
                        case 255:
                            SetColorAt(alpha, x, y, white);
                            conf[idx] = 255;
                            var fgColor = GetColorAt(image, x, y);
                            SetColorAt(foreground, x, y, fgColor);
                            break;
                    }
                }
            }
        }

        // Helper method to set the color at a specific location in the image array
        private void SetColorAt(byte[] imageData, int x, int y, byte[] color) {
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        private void SetColorAt(byte[] imageData, int x, int y, PixelRgb color) {
            int startIdx = (y * width + x) * bytesPerPixel;
            imageData[startIdx] = color.R;
            imageData[startIdx + 1] = color.G;
            imageData[startIdx + 2] = color.B;
        }

        // Helper method to compare the intensity of two colors
        private int IntensityComp(Point p0, Point p1) {
            var c0 = GetColorAt(image, p0.X, p0.Y);
            var c1 = GetColorAt(image, p1.X, p1.Y);

            int sumC0 = c0.R + c0.G + c0.B;
            int sumC1 = c1.R + c1.G + c1.B;
            
            return sumC0.CompareTo(sumC1);
        }

        public void RunGlobalMatting(byte[] trimap, out byte[] foreground, out byte[] alpha, out byte[] conf) {
            if (image == null || image.Length == 0)
                throw new ArgumentException("image is empty");
            if (trimap == null || trimap.Length == 0)
                throw new ArgumentException("trimap is empty");

            if (image.Length != width * height * bytesPerPixel)
                throw new ArgumentException("image must have CV_8UC3 type (3 channels) but was bytesPerPixel=" + bytesPerPixel);

            if (image.Length != trimap.Length)
                throw new ArgumentException("image and trimap must have the same size");

            GlobalMattingHelper(trimap, out foreground, out alpha, out conf);
        }

        public byte[] RunGuidedFilter(byte[] alpha, int r, double eps) {
            var imageGuidedFilter = new GuidedFilter(image, width, height, bytesPerPixel, r, eps);
            var guidedFilterInstance = imageGuidedFilter.Init(bytesPerPixel);
            var guidedIm = GuidedFilter.RunGuidedFilter(alpha, guidedFilterInstance);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    var col = GetColorAt(guidedIm, x, y);
                    var temp = new double[] { col.R, col.G, col.B };
                    var max = (byte)temp.Max();
                    SetColorAt(guidedIm, x, y, new[] { max, max, max, (byte)255 });
                }
            }

            return guidedIm;
        }
    }

}