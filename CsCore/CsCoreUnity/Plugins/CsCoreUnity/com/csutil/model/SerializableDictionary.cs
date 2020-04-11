using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.csutil.model {

    /// <summary>
    /// This is a dictionary that can be displayed in the Unity inspector UI. 
    /// A non generic subclass must be created for it to show correctly in the editor.
    /// Initial idea from http://answers.unity.com/answers/809221/view.html
    /// </summary>
    [Serializable]
    public class SerializableDictionary<K, V, E> : Dictionary<K, V>, ISerializationCallbackReceiver
        where E : SerializableEntry<K, V> {

        [SerializeField]
        private List<E> keyValuePairs = new List<E>();

        // save the dictionary to lists
        public void OnBeforeSerialize() {
            keyValuePairs.Clear();
            foreach (KeyValuePair<K, V> pair in this) {
                var e = Activator.CreateInstance<E>();
                e.key = pair.Key;
                e.value = pair.Value;
                keyValuePairs.Add(e);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize() {
            Clear();
            for (int i = 0; i < keyValuePairs.Count; i++) { Add(keyValuePairs[i].key, keyValuePairs[i].value); }
        }

    }

    [Serializable]
    public class SerializableEntry<K, V> {
        [SerializeField]
        public K key;
        [SerializeField]
        public V value;
    }

}
