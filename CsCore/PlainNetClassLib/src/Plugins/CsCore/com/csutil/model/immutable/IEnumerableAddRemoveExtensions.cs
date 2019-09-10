using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace com.csutil {

    public static class IEnumerableAddRemoveExtensions {

        /// <summary> Will not add the element if its already contained </summary>
        public static IEnumerable<T> AddViaUnion<T>(this IEnumerable<T> self, T value) {
            if (self is IImmutableList<T> l) { return l.Add(value); }
            return self.Union(new T[1] { value });
        }

        /// <summary> Will not add the elements if they are already contained </summary>
        public static IEnumerable<T> AddRangeViaUnion<T>(this IEnumerable<T> self, IEnumerable<T> newItems) {
            if (self is IImmutableList<T> l) { return l.AddRange(newItems); }
            return self.Union(newItems);
        }

        /// <summary> Will not insert the elements if they are already contained </summary>
        public static IEnumerable<T> InsertRangeViaUnion<T>(this IEnumerable<T> self, int index, IEnumerable<T> items) {
            if (self is IImmutableList<T> l) { return l.InsertRange(index, items); }
            var firstHalf = self.Take(index);
            var secondHalf = self.Skip(index);
            return firstHalf.Union(items).Union(secondHalf);
        }

        public static IEnumerable<T> RemoveRangeViaFilter<T>(this IEnumerable<T> self, IEnumerable<T> items) {
            if (self is ImmutableList<T> l) { return l.RemoveRange(items); }
            return self.Filter(x => !items.Contains(x));
        }

    }

}
