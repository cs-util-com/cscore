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

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> self, Func<T, bool> predicate) {
            return self.Where(predicate);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self) { return self == null || self.Count() == 0; }

        public static string ToStringV2<T>(this IEnumerable<T> args, Func<T, string> toString) {
            if (args == null) { return "null"; }
            if (args.IsNullOrEmpty()) { return "[]"; }
            return "[" + args.Map(x => "" + toString(x)).Reduce((x, y) => x + ", " + y) + "]";
        }

    }
}