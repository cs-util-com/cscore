using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model.immutable;

namespace com.csutil {

    public static class IEnumerableDiffHelpers {

        [Obsolete("Use CalcEntryChangesToOldStateV3 instead", true)]
        public static void CalcEntryChangesToOldStateV2<E, K, V>(this E newState, ref E oldState, Action<KeyValuePair<K, V>> onEntryAdded, Action<KeyValuePair<K, V>, KeyValuePair<K, V>> onEntryUpdated, Action<K> onEntryRemoved) where E : IReadOnlyDictionary<K, V> {
            CalcEntryChangesToOldState(newState, ref oldState, getKey: x => x.Key, onEntryAdded, onEntryUpdated, onEntryRemoved);
        }

        [Obsolete("Use CalcEntryChangesToOldStateV3 instead", true)]
        public static void CalcEntryChangesToOldState<E, K, V>(this E newState, ref E oldState, Action<KeyValuePair<K, V>> onEntryAdded, Action<KeyValuePair<K, V>, KeyValuePair<K, V>> onEntryUpdated, Action<K> onEntryRemoved) where E : IDictionary<K, V> {
            CalcEntryChangesToOldState(newState, ref oldState, getKey: x => x.Key, onEntryAdded, onEntryUpdated, onEntryRemoved);
        }

        /// <summary> Calculates the diff between an old state of an <see cref="IEnumerable{T}"/> and a new one </summary>
        /// <param name="oldState"> The old state is automatically reassigned during the comparison to avoid some bugs that otherwise can happen </param>
        /// <param name="getKey"> An ID/key - Must return a unique identifier of the object to correlate an old and new version of the same entry. E.g. a uuid or a timestamp that never changes </param>
        /// <param name="onEntryAdded"></param>
        /// <param name="onEntryUpdated"></param>
        /// <param name="onEntryRemoved"></param>
        public static void CalcEntryChangesToOldState<E, K, V>(this E newState, ref E oldState, Func<V, K> getKey, Action<V> onEntryAdded, Action<V, V> onEntryUpdated, Action<K> onEntryRemoved) where E : IEnumerable<V> {
            // TODO does not include order changes

            var oldStateCopy = oldState;
            oldState = newState;
            if (ReferenceEquals(oldStateCopy, newState)) { return; }
            if (oldStateCopy == null) {
                foreach (var x in newState) { onEntryAdded(x); }
                return;
            }
            if (newState == null) {
                foreach (var old in oldStateCopy) { onEntryRemoved(getKey(old)); }
                return;
            }

            var newDict = newState.ToDictionary(getKey, x => x);
            if (ReferenceEquals(newDict, newState)) { throw new InvalidOperationException("ToDictionary did not create a new temporary object"); }
            foreach (var old in oldStateCopy) {
                var keyOld = getKey(old);
                if (!newDict.ContainsKey(keyOld)) {
                    onEntryRemoved(keyOld);
                } else {
                    var foundMatch = newDict[keyOld];
                    if (StateCompare.WasModified(old, foundMatch)) { onEntryUpdated(old, foundMatch); }
                    newDict.Remove(keyOld);
                }
            }
            // The remaining ones in the newDict must be entries added:
            foreach (var newLeft in newDict) {
                onEntryAdded(newLeft.Value);
            }

        }

        public static void CalcEntryChangesToOldStateV3<E, K, V>(this E newState, ref E oldState, Action<K, V> onEntryAdded, Action<K, V, V> onEntryUpdated, Action<K> onEntryRemoved) where E : IDictionary<K, V> {

            var oldStateCopy = oldState;
            oldState = newState;
            if (ReferenceEquals(oldStateCopy, newState)) {
                Log.w("CalcEntryChangesToOldStateV3: oldStateCopy == newState");
                return;
            }
            if (oldStateCopy == null) {
                foreach (var kv in newState) { onEntryAdded(kv.Key, kv.Value); }
                return;
            }
            if (newState == null) {
                foreach (var oldKv in oldStateCopy) { onEntryRemoved(oldKv.Key); }
                return;
            }

            // Check for removed or updated entries:
            foreach (var oldKv in oldStateCopy) {
                K key = oldKv.Key;
                V oldVal = oldKv.Value;
                if (!newState.TryGetValue(key, out V newVal)) {
                    onEntryRemoved(key);
                } else {
                    if (StateCompare.WasModified(oldVal, newVal)) { onEntryUpdated(key, oldVal, newVal); }
                }
            }

            // Check for newly added entries:
            foreach (var newKv in newState) {
                K key = newKv.Key;
                if (oldStateCopy == null || !oldStateCopy.ContainsKey(key)) {
                    onEntryAdded(key, newKv.Value);
                }
            }
        }
        
        public static void CalcEntryChangesToOldStateV4<E, K, V>(this E newState, ref E oldState, Action<K, V> onEntryAdded, Action<K, V, V> onEntryUpdated, Action<K> onEntryRemoved) where E : IReadOnlyDictionary<K, V> {

            var oldStateCopy = oldState;
            oldState = newState;
            if (ReferenceEquals(oldStateCopy, newState)) {
                Log.w("CalcEntryChangesToOldStateV3: oldStateCopy == newState");
                return;
            }
            if (oldStateCopy == null) {
                foreach (var kv in newState) { onEntryAdded(kv.Key, kv.Value); }
                return;
            }
            if (newState == null) {
                foreach (var oldKv in oldStateCopy) { onEntryRemoved(oldKv.Key); }
                return;
            }

            // Check for removed or updated entries:
            foreach (var oldKv in oldStateCopy) {
                K key = oldKv.Key;
                V oldVal = oldKv.Value;
                if (!newState.TryGetValue(key, out V newVal)) {
                    onEntryRemoved(key);
                } else {
                    if (StateCompare.WasModified(oldVal, newVal)) { onEntryUpdated(key, oldVal, newVal); }
                }
            }

            // Check for newly added entries:
            foreach (var newKv in newState) {
                K key = newKv.Key;
                if (oldStateCopy == null || !oldStateCopy.ContainsKey(key)) {
                    onEntryAdded(key, newKv.Value);
                }
            }
        }
        

    }

}