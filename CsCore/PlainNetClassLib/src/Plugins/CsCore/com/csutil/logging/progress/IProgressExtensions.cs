using System;

namespace com.csutil.progress {

    public static class IProgressExtensions {

        public static void SetCount(this IProgress self, double current, double total) {
            self.totalCount = total;
            self.SetCount(current);
        }

        public static void SetCount(this IProgress self, double current) {
            if (self.totalCount == 0) { throw new DivideByZeroException("progress.totalCount not set"); }
            self.percent = 100d * current / self.totalCount;
            AssertV2.IsTrue(0 <= self.percent && self.percent <= 100, "" + self);
        }

        public static void IncrementCount(this IProgress self, int incrementStep = 1) {
            self.SetCount(self.GetCount() + incrementStep);
        }

        public static double GetCount(this IProgress self) {
            return self.totalCount * self.percent / 100d;
        }

        public static double GetCappedPercent(this IProgress self) {
            return Math.Min(100d, self.percent);
        }

        public static bool IsComplete(this IProgress self) { return self.percent >= 100; }
        public static void SetComplete(this IProgress self) { self.percent = 100; }

    }

}