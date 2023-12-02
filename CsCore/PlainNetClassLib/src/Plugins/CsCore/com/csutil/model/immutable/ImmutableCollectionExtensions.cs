using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace com.csutil.model.immutable {

    public static class ImmutableCollectionExtensions {

        public static Action AddStateChangeListenerForDictionary<T, K, V>(this IDataStore<T> self, Func<T, ImmutableDictionary<K, V>> getSubState, Action<KeyValuePair<K, V>> onEntryAdded, Action<KeyValuePair<K, V>> onEntryUpdated, Action<K> onEntryRemoved) {
            var oldDictionaryState = getSubState(self.GetState());
            return self.AddStateChangeListener(getSubState, (newDictionary) => {
                newDictionary.CalcEntryChangesToOldState(ref oldDictionaryState, onEntryAdded, onEntryUpdated, onEntryRemoved);
            }, triggerInstantToInit: false);
        }

    }

}