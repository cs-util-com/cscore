using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace com.csutil.model.immutable {

    public static class ImmutableCollectionExtensions {

        [Obsolete("Use AddStateChangeListenerForDictionaryV2 instead", true)]
        public static Action AddStateChangeListenerForDictionary<T, K, V>(this IDataStore<T> self, Func<T, ImmutableDictionary<K, V>> getSubState, Action<KeyValuePair<K, V>> onEntryAdded,
            Action<KeyValuePair<K, V>, KeyValuePair<K, V>> onEntryUpdated, Action<K> onEntryRemoved) {
            {
                var oldDictionaryState = getSubState(self.GetState());
                return self.AddStateChangeListener(getSubState, (newDictionary) => {
                    newDictionary.CalcEntryChangesToOldState(ref oldDictionaryState, onEntryAdded, onEntryUpdated, onEntryRemoved);
                }, triggerInstantToInit: false);
            }
        }
        
        public static Action AddStateChangeListenerForDictionaryV2<T, K, V>(this IDataStore<T> self, Func<T, ImmutableDictionary<K, V>> getSubState, Action<K, V> onEntryAdded,
            Action<K, V, V> onEntryUpdated, Action<K> onEntryRemoved) {
            {
                var oldDictionaryState = getSubState(self.GetState());
                return self.AddStateChangeListener(getSubState, (newDictionary) => {
                    newDictionary.CalcEntryChangesToOldStateV3(ref oldDictionaryState, onEntryAdded, onEntryUpdated, onEntryRemoved);
                }, triggerInstantToInit: false);
            }
        }

    }

}