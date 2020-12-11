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

        private const double REFRESH_RATE_IN_SECONDS = 1;
        private DateTime nextSerializeTime = ResetNextUpdateTimeStamp();
        private static DateTime ResetNextUpdateTimeStamp() {
            return DateTimeV2.UtcNow + TimeSpan.FromSeconds(REFRESH_RATE_IN_SECONDS);
        }

        // save the dictionary to lists
        public void OnBeforeSerialize() {
            if (WasRecentlySerialized()) {
                MakeSureKeysAreUniqueAndDeveloperFriendly();
                return;
            }
            keyValuePairs.Clear();
            foreach (KeyValuePair<K, V> pair in this) {
                var e = Activator.CreateInstance<E>();
                e.key = pair.Key;
                e.value = pair.Value;
                keyValuePairs.Add(e);
            }
        }

        /// <summary> In editor will only be true after some time to not save to often </summary>
        private bool WasRecentlySerialized() {
#if !UNITY_EDITOR
            return false; // If not in the editor never skip serialization
#endif
            bool wasRecentlySerialized = nextSerializeTime.IsAfter(DateTimeV2.UtcNow);
            if (!wasRecentlySerialized) { nextSerializeTime = ResetNextUpdateTimeStamp(); }
            return wasRecentlySerialized;
        }

        /// <summary> Unity has some intereting intialization logic of lists where it duplicates the 
        /// last list entry, since this list is actually representing a dictionary duplicating the 
        /// last entry is not intuitive and instead the default value should be set automatically, which 
        /// is what this method does </summary>
        private void MakeSureKeysAreUniqueAndDeveloperFriendly() {
            HashSet<K> foundKeys = new HashSet<K>();
            for (int i = 0; i < keyValuePairs.Count; i++) {
                var e = keyValuePairs[i];
                if (!foundKeys.Add(e.key)) {
                    if (typeof(string).IsCastableTo<K>()) {
                        e.key = (K)(object)("" + (i + 1));
                    } else {
                        e.key = default(K);
                    }
                    e.value = default(V);
                    // Make sure the updated values are persisted in the actual model:
                    this.Add(e.key, e.value);
                }
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize() {
            Clear();
            for (int i = 0; i < keyValuePairs.Count; i++) {
                this.AddOrReplace(keyValuePairs[i].key, keyValuePairs[i].value);
            }
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