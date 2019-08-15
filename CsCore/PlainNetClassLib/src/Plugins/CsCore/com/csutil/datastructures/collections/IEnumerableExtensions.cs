using System;
using System.Collections.Generic;
using System.Linq;

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

        public static void ForEach<T>(this IEnumerable<T> self, Action<T> predicate) {
            AssertV2.IsNotNull(self, "IEnumerable<" + typeof(T) + ">");
            foreach (var item in self) { predicate(item); }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self) { return self == null || !self.Any(); }

        public static string ToStringV2(this IEnumerable<string> args, string bracket1 = "[", string bracket2 = "]") {
            return args.ToStringV2<string>(x => x, bracket1, bracket2);
        }

        public static string ToStringV2<T>(this IEnumerable<T> args, Func<T, string> toString, string bracket1 = "[", string bracket2 = "]") {
            if (args == null) { return "null"; }
            if (args.IsNullOrEmpty()) { return bracket1 + bracket2; }
            var filteredResultStrings = args.Map(x => "" + toString(x)).Filter(x => !x.IsNullOrEmpty());
            if (filteredResultStrings.IsNullOrEmpty()) { return bracket1 + bracket2; }
            return bracket1 + filteredResultStrings.Reduce((x, y) => x + ", " + y) + bracket2;
        }

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) {
            var r = new HashSet<TSource>();
            foreach (var e in source) { r.Add(e); }
            return r;
        }

    }

}