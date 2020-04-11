using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            await Task.WhenAll(self.Select(async x => { if (await predicate(x)) { results.Enqueue(x); } }));
            return results;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self) { return self == null || !self.Any(); }

        public static string ToStringV2(this IEnumerable<string> args, string bracket1 = "[", string bracket2 = "]", string separator = ", ") {
            return args.ToStringV2<string>(x => x, bracket1, bracket2, separator);
        }

        public static string ToStringV2<T>(this IEnumerable<T> args, Func<T, string> toString, string bracket1 = "[", string bracket2 = "]", string separator = ", ") {
            if (args == null) { return "null"; }
            if (args.GetType() == typeof(string)) { return (string)(object)args; }
            if (args.IsNullOrEmpty()) { return bracket1 + bracket2; }
            var filteredResultStrings = args.Map(x => "" + toString(x)).Filter(x => !x.IsNullOrEmpty());
            if (filteredResultStrings.IsNullOrEmpty()) { return bracket1 + bracket2; }
            return bracket1 + filteredResultStrings.Reduce((x, y) => x + separator + y) + bracket2;
        }

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) {
            var r = new HashSet<TSource>();
            foreach (var e in source) { r.Add(e); }
            return r;
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<K> keys, IEnumerable<V> values) {
            return keys.Zip(values, (key, value) => new KeyValuePair<K, V>(key, value)).ToDictionary(p => p.Key, p => p.Value);
        }

        public static int IndexOf<T>(this IEnumerable<T> self, T elem) { return self.IndexOf(x => Equals(x, elem)); }

        public static int IndexOf<T>(this IEnumerable<T> self, Func<T, bool> predicate) {
            var index = -1;
            if (self.Any(x => { index++; return predicate(x); })) { return index; }
            return -1;
        }

    }

}