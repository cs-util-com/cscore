using System;
using System.Collections.Generic;
using System.Linq;
using StbImageSharp;

namespace com.csutil.algorithms.images {

    /// <summary> Ported version of https://raw.githubusercontent.com/atilimcetin/global-matting/master/globalmatting.cpp </summary>
    public class GlobalMatting {

        private readonly byte[] black = new byte[] { 0, 0, 0, 0 };
        private readonly byte[] almostBlack = new byte[] { 1, 1, 1, 1 };
        private readonly byte[] gray = new byte[] { 128, 128, 128, 128 };
        private readonly byte[] almostWhite = new byte[] { 254, 254, 254, 254 };
        private readonly byte[] white = new byte[] { 255, 255, 255, 255 };

        private readonly Random rand = new Random();

        private readonly byte[] image;
        private readonly int width;
        private readonly int height;
        private readonly int bytesPerPixel;

        public GlobalMatting(ImageResult image)
            : this(image.Data.DeepCopy(), image.Width, image.Height, (int)image.ColorComponents) {
        }

        public GlobalMatting(byte[] image, int width, int height, int bytesPerPixel) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;
        }

        private static double Sqr(double a) {
            return a * a;
        }

        public struct Point {
            public int X, Y;

            public Point(int x, int y) {
                X = x;
                Y = y;
            }

            public override string ToString() { return "(" + X + "," + Y + ")"; }

            public bool Equals(Point other) {
                return X == other.X && Y == other.Y;
            }

            public override bool Equals(object obj) {
                return obj is Point other && Equals(other);
            }

            public override int GetHashCode() {
                return (X * 397) ^ Y;
            }

        }

        private struct PixelRgb {
            public byte R, G, B;

            public PixelRgb(byte r, byte g, byte b) {
                R = r;
                G = g;
                B = b;
            }
        }

        // Method to find boundary pixels
        private List<Point> FindBoundaryPixels(byte[] trimap, byte alpha, byte borderAlpha) {
            List<Point> result = new List<Point>();
            for (int x = 1; x < width - 1; ++x) {
                for (int y = 1; y < height - 1; ++y) {
                    // trimap is a linear array with all pixels from all rows concatenated and each pixel has bytesPerPixel bytes/entries in the array
                    int idx = (y * width + x) * bytesPerPixel; // Calculate the index for the current pixel

                    // Check the current pixel's value and its neighbors to find boundary pixels
                    if (trimap[idx] == alpha) {
                        if (trimap[idx - width] == borderAlpha || // pixel above
                            trimap[idx + width] == borderAlpha || // pixel below
                            trimap[idx - bytesPerPixel] == borderAlpha || // pixel to the left
                            trimap[idx + bytesPerPixel] == borderAlpha) // pixel to the right
                        {
                            result.Add(new Point(x, y));
                        }
                    }
                }
            }
            return result;
        }

        // Calculate alpha value (Eq. 2 in the C++ code)
        private double CalculateAlpha(PixelRgb F, PixelRgb B, PixelRgb I) {
            double result = 0;
            double div = 1e-6d;
            result += (I.R - B.R) * (F.R - B.R);
            div += (F.R - B.R) * (F.R - B.R);
            result += (I.G - B.G) * (F.G - B.G);
            div += (F.G - B.G) * (F.G - B.G);
            result += (I.B - B.B) * (F.B - B.B);
            div += (F.B - B.B) * (F.B - B.B);
            return Math.Min(Math.Max(result / div, 0.0d), 1.0d);
        }

        // Color cost (Eq. 3 in the C++ code)
        private double ColorCost(PixelRgb F, PixelRgb B, PixelRgb I, double alpha) {
            double result = 0;
            double oneMinusAlpha = 1d - alpha;
            result += Sqr(I.R - (alpha * F.R + oneMinusAlpha * B.R));
            result += Sqr(I.G - (alpha * F.G + oneMinusAlpha * B.G));
            result += Sqr(I.B - (alpha * F.B + oneMinusAlpha * B.B));
            return Math.Sqrt(result);
        }

        // Distance cost between two points
        private static double DistCost(Point p0, Point p1, double minDist) {
            var a = p0.X - p1.X;
            var b = p0.Y - p1.Y;
            var dist = Math.Sqrt(a * a + b * b);
            return dist / minDist;
        }

        // Color distance between two pixels
        private static double ColorDist(PixelRgb I0, PixelRgb I1) {
            var r = I0.R - I1.R;
            var g = I0.G - I1.G;
            var b = I0.B - I1.B;
            return Math.Sqrt(r * r + g * g + b * b);
        }

        [Obsolete("Use NearestDistance instead")]
        private static double NearestDistanceOld(Dictionary<Point, double> boundary, Point p) {
            double minDist = double.MaxValue;
            foreach (var pBoundary in boundary.Keys) {
                var x = pBoundary.X - p.X;
                var y = pBoundary.Y - p.Y;
                var distanceSquared = x * x + y * y;
                minDist = Math.Min(minDist, distanceSquared);
            }
            return Math.Sqrt(minDist);
        }

        // Nearest distance from a point to any point in a boundary
        private double NearestDistance(Dictionary<Point, double> boundaryDistance, Point p) {
            while (true) {
                if (boundaryDistance.TryGetValue(p, out double dist)) { return dist; }
                // If the point is not in the boundary, expand the entries in the dict by 1 in all directions and check again
                var pointsArray = boundaryDistance.Keys.ToArray();
                foreach (var pointWithKnownDistance in pointsArray) {
                    var x = pointWithKnownDistance.X;
                    var y = pointWithKnownDistance.Y;
                    if (x < 0 || y < 0 || x >= width || y >= height) { continue; }
                    var knownBoundaryDist = boundaryDistance[pointWithKnownDistance];
                    // For the pointWithKnownDistance collect the distance entries in all 4 directions and use the lowest value
                    var left = new Point(x - 1, y);
                    if (!boundaryDistance.ContainsKey(left)) { boundaryDistance[left] = knownBoundaryDist + 1; }

                    var right = new Point(x + 1, y);
                    if (!boundaryDistance.ContainsKey(right)) { boundaryDistance[right] = knownBoundaryDist + 1; }

                    var up = new Point(x, y + 1);
                    if (!boundaryDistance.ContainsKey(up)) { boundaryDistance[up] = knownBoundaryDist + 1; }

                    var down = new Point(x, y - 1);
                    if (!boundaryDistance.ContainsKey(down)) { boundaryDistance[down] = knownBoundaryDist + 1; }
                }
            }
        }

        // TODO remove colorCache to allow parallel processing
        private readonly byte[] _colorCache = new byte[4];

        // Helper method to get color at a given position
        private PixelRgb GetColorAt(byte[] img, int x, int y) {
            // img is a linear array with all pixels from all rows concatenated and each pixel has bytesPerPixel bytes/entries in the array
            int startIdx = (y * width + x) * bytesPerPixel;
            return new PixelRgb(img[startIdx], img[startIdx + 1], img[startIdx + 2]);
        }

        // Method to expand known regions in the trimap with checks
        public void ExpansionOfKnownRegions(ref byte[] trimap, int niter) {
            var t = Log.MethodEnteredWith("niter=" + niter);
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
                double scalingFactor = (double)(niter - i) / niter;
                ExpansionOfKnownRegionsHelper(image, trimap, i + 1, scalingFactor);
            }
            ErodeFB(ref trimap, 2);
            Log.MethodDone(t);
        }

        // This method performs an expansion of known regions
        private void ExpansionOfKnownRegionsHelper(byte[] image, byte[] trimap, int r, double c) {
            int w = width;
            int h = height;
            // ... (Conversion logic) ...
            for (var x = 0; x < w; ++x) {
                for (var y = 0; y < h; ++y) {
                    if (!ColorIsValue(trimap, x, y, 128)) { continue; }

                    var im = GetColorAt(image, x, y);

                    for (var j = y - r; j <= y + r; ++j) {
                        for (var i = x - r; i <= x + r; ++i) {
                            if (i < 0 || i >= w || j < 0 || j >= h) { continue; }

                            if (!ColorIsValue(trimap, i, j, 0) && !ColorIsValue(trimap, i, j, 255)) { continue; }

                            var imCur = GetColorAt(image, i, j);
                            var pd = Math.Sqrt(Sqr(x - i) + Sqr(y - j));
                            var cd = ColorDist(im, imCur);

                            if (!(pd <= r) || !(cd <= c)) { continue; }

                            if (ColorIsValue(trimap, i, j, 0)) {
                                SetColorAt(trimap, x, y, almostBlack);
                            } else if (ColorIsValue(trimap, i, j, 255)) {
                                SetColorAt(trimap, x, y, almostWhite);
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
                        SetColorAt(background, x, y, almostBlack);
                    else if (ColorIsValue(trimap, x, y, 255))
                        SetColorAt(foreground, x, y, almostBlack);
                }
            }

            // Erode the foreground and background
            byte[] erodedBackground = Filter.Erode(background, w, h, 4, r);
            byte[] erodedForeground = Filter.Erode(foreground, w, h, 4, r);

            // Increase unknown region
            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    if (ColorIsValue(erodedBackground, x, y, 0) && ColorIsValue(erodedForeground, x, y, 0))
                        SetColorAt(trimap, x, y, gray); // Set to unknown
                }
            }
        }

        private bool ColorIsValue(byte[] array, int x, int y, int alpha) {
            // var color = GetColorAt(array, x, y);
            // return color.R == alpha && color.G == alpha && color.B == alpha;

            // Since the 3 channels are the same, we can just check the first channel: 
            int startIdx = (y * width + x) * bytesPerPixel;
            return array[startIdx] == alpha;
        }

        private struct Sample {
            public int foregroundListIndex, backgroundListIndex;
            public double df, db;
            public double cost, alpha;
        }

        // Method to calculate alpha patch match
        private void CalculateAlphaPatchMatch(byte[] trimap, List<Point> foregroundBoundary, List<Point> backgroundBoundary, out Sample[][] samples) {
            int w = width;
            int h = height;

            var t = Log.MethodEntered("Calc forground and background distances");
            samples = new Sample[h][];
            for (int i = 0; i < h; i++)
                samples[i] = new Sample[w];

            // For all gray/uncertain trimap values find the closest foreground and background pixels
            Dictionary<Point, double> foregroundBoundaryDict = foregroundBoundary.ToDictionary(p => p, p => 0d);
            Dictionary<Point, double> backgroundBoundaryDict = backgroundBoundary.ToDictionary(p => p, p => 0d);
            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    if (ColorIsValue(trimap, x, y, 128)) {
                        Point p = new Point(x, y);
                        samples[y][x].foregroundListIndex = rand.Next(foregroundBoundary.Count);
                        samples[y][x].backgroundListIndex = rand.Next(backgroundBoundary.Count);
                        samples[y][x].df = NearestDistance(foregroundBoundaryDict, p);
                        samples[y][x].db = NearestDistance(backgroundBoundaryDict, p);
                        samples[y][x].cost = double.MaxValue;
                    }
                }
            }
            Log.MethodDone(t);

            var t2 = Log.MethodEntered("Propagate iterations");
            var iterationCount = 10;
            for (int i = 0; i < iterationCount; ++i) {
                DoPropagateIteration(trimap, foregroundBoundary, backgroundBoundary, samples, w, h);
            }
            Log.MethodDone(t2);
        }

        private void DoPropagateIteration(byte[] trimap, List<Point> foregroundBoundary, List<Point> backgroundBoundary, Sample[][] samples, int w, int h) {

            // Create and shuffle coordinate points
            List<Point> coords1 = new List<Point>(h * w);
            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    coords1.Add(new Point(x, y));

            var shuffeledCoords = rand.ShuffleEntries(coords1);
            foreach (Point p in shuffeledCoords) {
                int x = p.X;
                int y = p.Y;

                // Only look at the uncertain(gray) pixels in the trimap
                if (!ColorIsValue(trimap, x, y, 128)) { continue; }

                var I = GetColorAt(image, x, y);

                Sample s = samples[y][x];

                // Propagation: Check the neighbors' samples
                for (int y2 = y - 1; y2 <= y + 1; ++y2) {
                    for (int x2 = x - bytesPerPixel; x2 <= x + bytesPerPixel; x2 += bytesPerPixel) {
                        if (x2 < 0 || x2 >= w || y2 < 0 || y2 >= h)
                            continue;

                        if (!ColorIsValue(trimap, x2, y2, 128))
                            continue;

                        Sample s2 = samples[y2][x2];

                        // ... Calculate cost ...
                        Point fp = foregroundBoundary[s2.foregroundListIndex];
                        Point bp = backgroundBoundary[s2.backgroundListIndex];
                        var F = GetColorAt(image, fp.X, fp.Y);
                        var B = GetColorAt(image, bp.X, bp.Y);
                        double alpha = CalculateAlpha(F, B, I);
                        double cost = ColorCost(F, B, I, alpha) + DistCost(p, fp, s.df) + DistCost(p, bp, s.db);

                        // If new cost is lower, update the sample
                        if (cost < s.cost) {
                            s.foregroundListIndex = s2.foregroundListIndex;
                            s.backgroundListIndex = s2.backgroundListIndex;
                            s.cost = cost;
                            s.alpha = alpha;
                        }
                    }
                }

                // Random walk
                int w2 = Math.Max(foregroundBoundary.Count, backgroundBoundary.Count);

                for (int k = 0;; k++) {
                    double r = w2 * Math.Pow(0.5, k);

                    if (r < 1) { break; }

                    int di = (int)(r * rand.NextDouble());
                    int dj = (int)(r * rand.NextDouble());

                    int fi = s.foregroundListIndex + (rand.NextDouble() > 0.5 ? di : -di);
                    int bj = s.backgroundListIndex + (rand.NextDouble() > 0.5 ? dj : -dj);

                    if (fi < 0 || fi >= foregroundBoundary.Count || bj < 0 || bj >= backgroundBoundary.Count) { continue; }

                    Point fp = foregroundBoundary[fi];
                    Point bp = backgroundBoundary[bj];

                    var F = GetColorAt(image, fp.X, fp.Y);
                    var B = GetColorAt(image, bp.X, bp.Y);

                    double alpha = CalculateAlpha(F, B, I);
                    double cost = ColorCost(F, B, I, alpha) + DistCost(p, fp, s.df) + DistCost(p, bp, s.db);

                    // If new cost is lower, update the sample
                    if (cost < s.cost) {
                        s.foregroundListIndex = fi;
                        s.backgroundListIndex = bj;
                        s.cost = cost;
                        s.alpha = alpha;
                    }
                }

                // After propagation and random walk, assign the sample back
                samples[y][x] = s;
            }
        }

        private void GlobalMattingHelper(byte[] trimap, out byte[] foreground, out byte[] alpha, out byte[] conf) {
            var t = Log.MethodEntered();
            var foregroundBoundary = FindBoundaryPixels(trimap, 255, 128);
            var backgroundBoundary = FindBoundaryPixels(trimap, 0, 128);

            int n = foregroundBoundary.Count + backgroundBoundary.Count;
            for (int i = 0; i < n; ++i) {
                int x = rand.Next(width);
                int y = rand.Next(height);
                if (ColorIsValue(trimap, x, y, 0))
                    backgroundBoundary.Add(new Point(x, y));
                else if (ColorIsValue(trimap, x, y, 255))
                    foregroundBoundary.Add(new Point(x, y));
            }
            foregroundBoundary = foregroundBoundary.Distinct().ToList();
            backgroundBoundary = backgroundBoundary.Distinct().ToList();

            foregroundBoundary.Sort((p0, p1) => IntensityComp(p0, p1));
            backgroundBoundary.Sort((p0, p1) => IntensityComp(p0, p1));

            var t2 = Log.MethodEntered("CalculateAlphaPatchMatch");
            CalculateAlphaPatchMatch(trimap, foregroundBoundary, backgroundBoundary, out var samples);
            Log.MethodDone(t2);

            // Initialize output arrays
            foreground = new byte[width * height * bytesPerPixel];
            alpha = new byte[width * height * bytesPerPixel];
            conf = new byte[width * height * bytesPerPixel];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    // trimap is a linear array with all pixels from all rows concatenated and each pixel has bytesPerPixel bytes/entries in the array
                    int idx = (y * width + x) * bytesPerPixel;
                    switch (trimap[idx]) {
                        case 0:
                            SetColorAt(alpha, x, y, black);
                            conf[idx] = 255;
                            SetColorAt(foreground, x, y, black);
                            break;
                        case 128:
                            Sample sample = samples[y][x];
                            byte alphaValue = (byte)(255d * sample.alpha);
                            _colorCache[0] = alphaValue;
                            _colorCache[1] = alphaValue;
                            _colorCache[2] = alphaValue;
                            _colorCache[3] = 255;
                            SetColorAt(alpha, x, y, _colorCache);
                            conf[idx] = (byte)(255 * Math.Exp(-sample.cost / 6d));
                            Point p = foregroundBoundary[sample.foregroundListIndex];
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
            Log.MethodDone(t);
        }

        // Helper method to set the color at a specific location in the image array
        private void SetColorAt(byte[] imageData, int x, int y, byte[] color) {
            // imageData is a linear array with all pixels from all rows concatenated and each pixel has bytesPerPixel bytes/entries in the array
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        private void SetColorAt(byte[] imageData, int x, int y, PixelRgb color) {
            // imageData is a linear array with all pixels from all rows concatenated and each pixel has bytesPerPixel bytes/entries in the array
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

        public byte[] RunGuidedFilter(byte[] alpha, int radius, double eps) {
            var imageGuidedFilter = new GuidedFilter(image, width, height, bytesPerPixel, radius, eps);
            var guidedFilterInstance = imageGuidedFilter.Init(bytesPerPixel);
            var guidedIm = GuidedFilter.RunGuidedFilter(alpha, guidedFilterInstance);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    var col = GetColorAt(guidedIm, x, y);
                    var max = Math.Max(col.R, Math.Max(col.G, col.B));
                    SetColorAt(guidedIm, x, y, new PixelRgb(max, max, max));
                }
            }
            return guidedIm;
        }

    }

}