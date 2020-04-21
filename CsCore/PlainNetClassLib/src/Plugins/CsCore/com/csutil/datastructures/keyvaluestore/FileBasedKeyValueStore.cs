using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.keyvaluestore {

    public class FileBasedKeyValueStore : IKeyValueStore {

        /// <summary> Will create a new store instance </summary>
        /// <param name="dirName"> e.g. "MyPersistedElems1" </param>
        public static FileBasedKeyValueStore New(string dirName) {
            return New(EnvironmentV2.SanatizeToFileName(EnvironmentV2.instance.systemInfo.appId), dirName);
        }

        public static FileBasedKeyValueStore New(string appId, string dirName) {
            return new FileBasedKeyValueStore(EnvironmentV2.instance.GetOrAddAppDataFolder(appId).GetChildDir(dirName));
        }

        public DirectoryEntry folderForAllFiles { get; private set; }
        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }

        private class PrimitiveWrapper { public object val; }

        public FileBasedKeyValueStore(DirectoryEntry folderForAllFiles) { this.folderForAllFiles = folderForAllFiles; }

        public void Dispose() { fallbackStore?.Dispose(); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = this.StartFallbackStoreGetTimer();
            Task<T> fallbackGet = fallbackStore.Get(key, defaultValue, (newVal) => InternalSet(key, newVal));
            await this.WaitLatestFallbackGetTime(s, fallbackGet);

            var fileForKey = GetFile(key);
            if (TryInternalGet(fileForKey, typeof(T), out object result)) { return (T)result; }
            return await fallbackGet;
        }

        private bool TryInternalGet(FileEntry fileForKey, Type type, out object result) {
            if (fileForKey.IsNotNullAndExists()) {
                try {
                    if (type.IsPrimitive) {
                        result = fileForKey.LoadAs<PrimitiveWrapper>().val;
                        return true;
                    }
                    result = fileForKey.LoadAs(type);
                    return true;
                }
                catch (Exception e) { Log.e(e); }
            }
            result = null;
            return false;
        }

        public FileEntry GetFile(string key) { return folderForAllFiles.GetChild(EnvironmentV2.SanatizeToFileName(key)); }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            var objType = value.GetType();
            if (objType.IsPrimitive) { value = new PrimitiveWrapper() { val = value }; }
            var file = GetFile(key);
            TryInternalGet(file, objType, out object oldVal);
            if (objType == typeof(string)) {
                file.SaveAsText((string)value);
            } else {
                file.SaveAsText(JsonWriter.GetWriter().Write(value));
            }
            return oldVal;
        }

        public async Task<bool> Remove(string key) {
            var res = GetFile(key).DeleteV2();
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            folderForAllFiles.DeleteV2();
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public async Task<bool> ContainsKey(string key) {
            if (GetFile(key).IsNotNullAndExists()) { return true; }
            if (fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return false;
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            if (!folderForAllFiles.Exists) { return Enumerable.Empty<string>(); }
            var result = folderForAllFiles.GetFiles().Map(x => x.Name);
            return await fallbackStore.ConcatAllKeys(result);
        }

    }

}