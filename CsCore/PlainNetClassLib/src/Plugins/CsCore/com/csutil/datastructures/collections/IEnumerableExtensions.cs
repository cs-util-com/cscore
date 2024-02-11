using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace com.csutil {

    public static class IEnumerableExtensions {

        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> self, Func<T, R> selector) {
            return self.Select(selector);
        }

        public static T Reduce<T>(this IEnumerable<T> self, Func<T, T, T> func) {
            return self.Aggregate(func);
        }

        public static R Reduce<T, R>(this IEnumerable<T> self, R seed, Func<R, T, R> func) {
            return self.Aggregate<T, R>(seed, func);
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> self, Func<T, bool> predicate) {
            return self.Where(predicate);
        }

        public static async Task<IEnumerable<R>> MapAsync<T, R>(this IEnumerable<T> self, Func<T, Task<R>> selector) {
            return await Task.WhenAll(self.Select(selector));
        }

        public static async Task<IEnumerable<T>> FilterAsync<T>(this IEnumerable<T> self, Func<T, Task<bool>> predicate) {
            var results = new ConcurrentQueue<T>();
            await Task.WhenAll(self.Select(async x => {
                if (await predicate(x)) { results.Enqueue(x); }
            }));
            return results;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> self) { return !self.Any(); }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self) {
            if (self == null) { return true; }
            if (self is ICollection<T> c) { return c.Count == 0; }
            return !self.Any();
        }

        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> self) { return self ?? Enumerable.Empty<T>(); }

        public static void ThrowErrorIfNullOrEmpty<T>(this IEnumerable<T> self, string paramName, [CallerMemberName] string methodName = null) {
            if (self.IsNullOrEmpty()) {
                System.Diagnostics.Debugger.Break();
                throw new ArgumentException($"{paramName} null or emtpy (Method {methodName})");
            }
        }

        public static string ToStringV2(this IEnumerable<string> args, string bracket1 = "[", string bracket2 = "]", string separator = ", ") {
            return args.ToStringV2<string>(x => x, bracket1, bracket2, separator);
        }

        public static string ToStringV2<T>(this IEnumerable<T> args, Func<T, string> toString, string bracket1 = "[", string bracket2 = "]", string separator = ", ") {
            if (args == null) { return "null"; }
            if (args is string) { return (string)(object)args; }
            if (args.IsNullOrEmpty()) { return bracket1 + bracket2; }
            var filteredResultStrings = args.Map(x => "" + toString(x)).Filter(x => !x.IsNullOrEmpty());
            if (filteredResultStrings.IsNullOrEmpty()) { return bracket1 + bracket2; }
            return bracket1 + filteredResultStrings.Reduce((x, y) => x + separator + y) + bracket2;
        }

        public static string ToStringV2FullJson<T>(this IEnumerable<T> self) {
            if (self is string) { return (string)(object)self; }
            if (self is IEnumerable<string> s) { return s.ToStringV2(); }
            return self.ToStringV2(c => JsonWriter.AsPrettyString(c));
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<K> keys, IEnumerable<V> values) {
            return keys.Zip(values, (key, value) => new KeyValuePair<K, V>(key, value)).ToDictionary(p => p.Key, p => p.Value);
        }

        public static Dictionary<K, W> MapValues<K, V, W>(this IDictionary<K, V> self, Func<V, W> mapFunc) {
            return self.ToDictionary(x => x.Key, x => mapFunc(x.Value));
        }

        public static int IndexOf<T>(this IEnumerable<T> self, T elem) { return self.IndexOf(x => Equals(x, elem)); }

        public static int IndexOf<T>(this IEnumerable<T> self, Func<T, bool> predicate) {
            var index = -1;
            if (self.Any(x => {
                    index++;
                    return predicate(x);
                })) { return index; }
            return -1;
        }

        public static IEnumerable<T> Move<T>(this IReadOnlyList<T> list, T entryToMove, int newIndex) {
            if (list == null || !list.Any())
                throw new ArgumentNullException(nameof(list));

            if (newIndex < 0 || newIndex > list.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(newIndex), $"New index is out of range (0 - {list.Count - 1}), but was {newIndex}");

            var currentIndex = list.IndexOf(entryToMove);

            // If the entry is not found or the new index is the same as the current index, return the list as is:
            if (currentIndex == -1) {
                throw new ArgumentException("The entry to move doesn't exist in the list");
            }
            // If the new index is the same as the current index, return the list as is:
            if (currentIndex == newIndex) { return list; }

            var resultList = list.ToList();
            resultList.RemoveAt(currentIndex);
            resultList.Insert(newIndex, entryToMove);
            return resultList;
        }

        public static void Move<T>(this IList<T> self, int oldIndex, int newIndex) {
            if (oldIndex == newIndex) { return; }
            T itemToMove = self[oldIndex];
            if (newIndex < 0 || self.Count - 1 < newIndex) {
                throw new ArgumentOutOfRangeException("newIndex", $"Cant move '{itemToMove}' from {oldIndex} to {newIndex} (>list.Count={self.Count})");
            }
            if (self is Array array) { // If the IList has a fixed size
                if (newIndex < oldIndex) { // Need to move part of the array "up" to make room
                    Array.Copy(array, newIndex, array, newIndex + 1, oldIndex - newIndex);
                } else { // Need to move part of the array "down" to fill the gap
                    Array.Copy(array, oldIndex + 1, array, oldIndex, newIndex - oldIndex);
                }
                array.SetValue(itemToMove, newIndex);
            } else { // If its not an array (has no fixed size, moving is easier:
                self.RemoveAt(oldIndex);
                self.Insert(newIndex, itemToMove);
            }
        }

        public static IEnumerable<T> Cached<T>(this IEnumerable<T> source) {
            if (source == null) { throw new ArgumentNullException("source"); }
            
            // Check for a few IEnumerable subtypes that dont require to be cached:
            if (source is IReadOnlyCollection<T>) { return source; }
            if (source is ICollection<T>) { return source; }
            if (source is ICollection) { return source; }
            
            if (source is CachedEnumerable<T>) { return source; }
            return new CachedEnumerable<T>(source);
        }

        /// <summary> Caches an ienumerable so that it can be traversed multiple times, needed eg for 
        /// yield enumerables if multiple traversales are required. See https://stackoverflow.com/a/12428250/165106 </summary>
        private class CachedEnumerable<T> : IEnumerable<T> {

            readonly Object gate = new Object();
            readonly IEnumerable<T> source;
            readonly List<T> cache = new List<T>();
            IEnumerator<T> enumerator;
            bool isCacheComplete;

            public CachedEnumerable(IEnumerable<T> source) { this.source = source; }

            public IEnumerator<T> GetEnumerator() {
                lock (this.gate) {
                    if (this.isCacheComplete)
                        return this.cache.GetEnumerator();
                    if (this.enumerator == null)
                        this.enumerator = source.GetEnumerator();
                }
                return GetCacheBuildingEnumerator();
            }

            public IEnumerator<T> GetCacheBuildingEnumerator() {
                var index = 0;
                T item;
                while (TryGetItem(index, out item)) {
                    yield return item;
                    index += 1;
                }
            }

            bool TryGetItem(Int32 index, out T item) {
                lock (this.gate) {
                    if (!IsItemInCache(index)) {
                        // The iteration may have completed while waiting for the lock.
                        if (this.isCacheComplete) {
                            item = default(T);
                            return false;
                        }
                        if (!this.enumerator.MoveNext()) {
                            item = default(T);
                            this.isCacheComplete = true;
                            this.enumerator.Dispose();
                            return false;
                        }
                        this.cache.Add(this.enumerator.Current);
                    }
                    item = this.cache[index];
                    return true;
                }
            }

            bool IsItemInCache(Int32 index) {
                return index < this.cache.Count;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

        }

        public static double GetVariance(this IEnumerable<double> self) {
            var values = self.Cached();
            return CalcVariance(values, values.Average());
        }

        public static double GetStandardDeviation(this IEnumerable<double> self) { return Math.Sqrt(GetVariance(self)); }

        public static double GetRelativeStandardDeviation(this IEnumerable<double> self) {
            var values = self.Cached();
            double average = values.Average();
            return Math.Abs(Math.Sqrt(CalcVariance(values, average)) / average);
        }

        private static double CalcVariance(IEnumerable<double> self, double average) {
            double sumOfSquaresOfDifferences = self.Sum(val => (val - average) * (val - average));
            return sumOfSquaresOfDifferences / (self.Count() - 1);
        }

        public static bool SequenceReferencesEqual<T>(this IEnumerable<T> self, IEnumerable<T> other) {
            using (IEnumerator<T> iterator1 = self.GetEnumerator()) {
                using (IEnumerator<T> iterator2 = other.GetEnumerator()) {
                    while (true) {
                        bool hasNext1 = iterator1.MoveNext();
                        bool hasNext2 = iterator2.MoveNext();
                        if (hasNext1 != hasNext2) { return false; } // Not same lenght
                        if (!hasNext1) { break; } // Both iterators reached the end
                        // Ensure that the references of the current 2 entries are the same: 
                        if (!ReferenceEquals(iterator1.Current, iterator2.Current)) { return false; }
                    }

                }
            }
            return true;
        }

        public static bool CountIsAbove(this IEnumerable self, int x) {
            if (self is ICollection l) { return l.Count > x; }
            var e = self.GetEnumerator();
            var count = 0;
            do {
                if (count > x) { return true; }
                count++;
            } while (e.MoveNext());
            return false;
        }

        public static bool CountIsBelow(this IEnumerable self, int x) {
            if (self is ICollection l) { return l.Count < x; }
            var e = self.GetEnumerator();
            var count = 0;
            do {
                if (count >= x) { return false; }
                count++;
            } while (e.MoveNext());
            return true;
        }

        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator) {
            return _ToIEnumerable(enumerator).Cached();
        }

        private static IEnumerable<T> _ToIEnumerable<T>(this IEnumerator<T> enumerator) {
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }

        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator enumerator) {
            return _ToIEnumerable(enumerator).Cast<T>().Cached();
        }

        private static IEnumerable _ToIEnumerable(this IEnumerator enumerator) {
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> self) {
            return new Queue<T>(self);
        }

    }

}

// Moved to separate namespace to not cause problems if a project uses TargetFramework netstandard2.1
namespace com.csutil.netstandard2_1polyfill {

    public static class IEnumerableExtensions2_1 {

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) {
            var r = new HashSet<TSource>();
            foreach (var e in source) { r.Add(e); }
            return r;
        }

    }

}