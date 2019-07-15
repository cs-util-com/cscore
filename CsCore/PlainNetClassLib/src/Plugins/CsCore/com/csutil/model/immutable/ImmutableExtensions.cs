using System;
using System.Collections.Immutable;

namespace com.csutil.model.immutable {
    public static class ImmutableExtensions {

        public static ImmutableList<T> MutateEntries<T>(this ImmutableList<T> list, object action, StateReducer<T> reducer) {
            if (list != null) {
                list.ForEach(elem => {
                    var newElem = reducer(elem, action);
                    if (!Object.Equals(newElem, elem)) { list = list.Replace(elem, newElem); }
                });
            }
            return list;
        }

        public static ImmutableList<T> AddOrCreate<T>(this ImmutableList<T> self, T t) { return (self == null) ? ImmutableList.Create(t) : self.Add(t); }

        public static T Mutate<T>(this T self, object action, StateReducer<T> reducer, ref bool changed) {
            return self.Mutate<T>(true, action, reducer, ref changed);
        }

        public static T Mutate<T>(this T self, bool applyReducer, object action, StateReducer<T> reducer, ref bool changed) {
            if (!applyReducer) { return self; }
            var newVal = reducer(self, action);
            changed = changed || !Object.Equals(self, newVal);
            return newVal;
        }

    }
}