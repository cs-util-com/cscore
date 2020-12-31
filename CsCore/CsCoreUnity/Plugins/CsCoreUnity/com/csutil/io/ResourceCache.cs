using com.csutil.model;
using System;
using System.Linq;
using UnityEngine;

namespace com.csutil.io {

    public class ResourceCache : MonoBehaviour {

        public static bool TryLoad<T>(string pathInResourcesFolder, out T result) {
            var caches = ResourcesV2.FindAllInScene<ResourceCache>();
            foreach (var cache in caches) {
                if (cache.Find(pathInResourcesFolder, out T res)) {
                    result = res;
                    return true;
                }
            }
            result = default(T);
            return false;
        }

        [Serializable]
        public class ObjDictEntry : SerializableEntry<string, UnityEngine.Object> {
        }
        [Serializable]
        public class ObjDict : SerializableDictionary<string, UnityEngine.Object, ObjDictEntry> { }

        public ObjDict cache;

        private bool Find<T>(string pathInResourcesFolder, out T res) {
            if (cache != null) {
                if (cache.TryGetValue(pathInResourcesFolder, out UnityEngine.Object r)) {
                    if (r is T t) {
                        res = t;
                        return true;
                    } else {
                        Log.e($"Entry with correct pathInResourcesFolder='{pathInResourcesFolder}' was " +
                            $"found but type was {r.GetType()} instead of {typeof(T)}");
                    }
                }
            }
            res = default(T);
            return false;
        }

        private void OnValidate() {
            if (cache == null) { cache = new ObjDict(); }
            var emtpyOnes = cache.Filter(o => IsDefaultKey(o.Key) && o.Value != null).ToList();
            foreach (var x in emtpyOnes) {
                cache.Add(CalcKey(x.Value), x.Value);
                cache.Remove(x.Key);
            }
        }

        private static bool IsDefaultKey(string key) {
            return key.IsNullOrEmpty() || int.TryParse(key, out _);
        }

        /// <summary> Tries to guess the correct pass the cached object would have in the Resources folder </summary>
        private string CalcKey(UnityEngine.Object value) { return "" + value.name; }

    }

}