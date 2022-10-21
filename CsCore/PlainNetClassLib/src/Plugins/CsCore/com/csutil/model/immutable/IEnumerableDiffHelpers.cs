using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model.immutable;

namespace com.csutil {

    public static class IEnumerableDiffHelpers {

        public static void CalcEntryChanges<K, V>(this IDictionary<K, V> oldState, IDictionary<K, V> newState, Action<KeyValuePair<K, V>> onEntryAdded, Action<KeyValuePair<K, V>> onEntryUpdated, Action<K> onEntryRemoved) {
            CalcEntryChanges(oldState, newState, getKey: x => x.Key, onEntryAdded, onEntryUpdated, onEntryRemoved);
        }

        /// <summary> Calculates the diff between an old state of an <see cref="IEnumerable{T}"/> and a new one </summary>
        /// <param name="getKey"> An ID/key - Must return a unique identifier of the object to correlate an old and new version of the same entry. E.g. a uuid or a timestamp that never changes </param>
        /// <param name="onEntryAdded"></param>
        /// <param name="onEntryUpdated"></param>
        /// <param name="onEntryRemoved"></param>
        public static void CalcEntryChanges<T, K>(this IEnumerable<T> oldState, IEnumerable<T> newState, Func<T, K> getKey, Action<T> onEntryAdded, Action<T> onEntryUpdated, Action<K> onEntryRemoved) {
            // TODO does not include order changes

            if (ReferenceEquals(oldState, newState)) { return; }
            if (oldState == null) {
                foreach (var x in newState) { onEntryAdded(x); }
                return;
            }
            if (newState == null) {
                foreach (var old in oldState) { onEntryRemoved(getKey(old)); }
                return;
            }

            var newDict = newState.ToDictionary(getKey, x => x);
            if (ReferenceEquals(newDict, newState)) { throw new InvalidOperationException("ToDictionary did not create a new temporary object"); }
            foreach (var old in oldState) {
                var keyOld = getKey(old);
                if (!newDict.ContainsKey(keyOld)) {
                    onEntryRemoved(keyOld);
                } else {
                    var foundMatch = newDict[keyOld];
                    if (StateCompare.WasModified(old, foundMatch)) { onEntryUpdated(foundMatch); }
                    newDict.Remove(keyOld);
                }
            }
            // The remaining ones in the newDict must be entries added:
            foreach (var newLeft in newDict) {
                onEntryAdded(newLeft.Value);
            }

        }

    }

}