using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.math {

    /// <summary>
    /// Calculates median in O(n) time. The generalized version of this problem is known as "n-order statistics" which
    /// means finding an element K in a set such that we have n elements smaller or equal to K and rest are larger or
    /// equal K. So 0th order statistic would be minimal element in the set (Note: Some literature use index from 1 to
    /// N instead of 0 to N-1). Median is simply (Count-1)/2-order statistic.
    /// From https://stackoverflow.com/a/22702269/165106 
    /// </summary>
    public static class CalculateMedian {

        public static float CalcMedian(this IEnumerable<float> self) {
            return (float)CalcMedian(self, x => x);
        }

        public static double CalcMedian(this IEnumerable<double> self) {
            return CalcMedian(self, x => x);
        }

        public static double CalcMedian<T>(this IEnumerable<T> sequence, Func<T, double> getValue) {
            var list = sequence.Select(getValue).ToList();
            if (list.IsEmpty()) { return double.NaN; }
            var mid = (list.Count - 1) / 2;
            return list.NthOrderStatistic(mid);
        }

        /// <summary>
        /// Partitions the given list around a pivot element such that all elements on left of pivot are <= pivot
        /// and the ones at thr right are > pivot. This method can be used for sorting, N-order statistics such as
        /// as median finding algorithms.
        /// Pivot is selected ranodmly if random number generator is supplied else its selected as last element in the list.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 171
        /// </summary>
        private static int Partition<T>(this IList<T> list, int start, int end, Random rnd = null) where T : IComparable<T> {
            if (rnd != null)
                list.Swap(end, rnd.Next(start, end + 1));

            var pivot = list[end];
            var lastLow = start - 1;
            for (var i = start; i < end; i++) {
                if (list[i].CompareTo(pivot) <= 0)
                    list.Swap(i, ++lastLow);
            }
            list.Swap(end, ++lastLow);
            return lastLow;
        }

        /// <summary>
        /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
        /// Note: specified list would be mutated in the process.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 216
        /// </summary>
        private static T NthOrderStatistic<T>(this IList<T> list, int n, Random rnd = null) where T : IComparable<T> {
            return NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
        }
        private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random rnd) where T : IComparable<T> {
            while (true) {
                var pivotIndex = list.Partition(start, end, rnd);
                if (pivotIndex == n)
                    return list[pivotIndex];

                if (n < pivotIndex)
                    end = pivotIndex - 1;
                else
                    start = pivotIndex + 1;
            }
        }

        private static void Swap<T>(this IList<T> list, int i, int j) {
            if (i == j) //This check is not required but Partition function may make many calls so its for perf reason
                return;
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static Tuple<T, T> CalcWeightedMedian<T>(this IEnumerable<T> self, Func<T, double> getWeight) where T : IComparable { // Sort the elements by their value
            // Idea from https://stackoverflow.com/a/9794746/165106 
            var orderedElems = self.OrderBy(x => x).ToList(); // Sort the element list by their values (not the weights)
            double totalWeight = orderedElems.Sum(e => getWeight(e)); // Calculate the total weight
            int k = 0;
            double sum = totalWeight - getWeight(orderedElems[0]); // sum is the total weight of all `x[i] > x[k]`
            while (sum > totalWeight / 2) {
                ++k;
                sum -= getWeight(orderedElems[k]);
            }
            if (orderedElems.Count % 2 == 0) {
                return new Tuple<T, T>(orderedElems[k], orderedElems[k + 1]);
            }
            return new Tuple<T, T>(orderedElems[k], orderedElems[k]);
        }

    }

}