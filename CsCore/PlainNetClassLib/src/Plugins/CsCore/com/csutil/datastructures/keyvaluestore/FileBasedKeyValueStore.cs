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
        [Obsolete("Use constructor directly")]
        public static FileBasedKeyValueStore New(string dirName) {
            return New(EnvironmentV2.instance.systemInfo.appId, dirName);
        }

        [Obsolete("Use constructor directly")]
        public static FileBasedKeyValueStore New(string appId, string dirName) {
            return new FileBasedKeyValueStore(EnvironmentV2.instance.GetOrAddAppDataFolder(appId).GetChildDir(dirName));
        }

        public DirectoryEntry folderForAllFiles { get; protected set; }
        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }

        private class PrimitiveWrapper { public object val; }
        /// <summary> Lock will be set while accessing the folder </summary>
        private object folderAccessLock = new object();
        private int openChanges = 0;


        public FileBasedKeyValueStore(DirectoryEntry folderForAllFiles) { this.folderForAllFiles = folderForAllFiles; }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        protected virtual int FlushOpenChangesIfNeeded(int openChanges) {
            // Changes to a normal directory are always instant persisted
            return 0; // Reset openChangesCounter
        }

        public virtual void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            fallbackStore?.Dispose();
            IsDisposed = DisposeState.Disposed;
        }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = this.StartFallbackStoreGetTimer();
            Task<T> fallbackGet = fallbackStore.Get(key, defaultValue, (newVal) => InternalSet(key, newVal));
            await this.WaitLatestFallbackGetTime(s, fallbackGet);
            lock (folderAccessLock) {
                var fileForKey = GetFile(key);
                if (TryInternalGet(fileForKey, typeof(T), out object result)) { return (T)result; }
            }
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

        public FileEntry GetFile(string key) { return folderForAllFiles.GetChild(key); }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            var objType = value.GetType();
            if (objType.IsPrimitive) { value = new PrimitiveWrapper() { val = value }; }
            lock (folderAccessLock) {
                var file = GetFile(key);
                TryInternalGet(file, objType, out object oldVal);
                if (objType == typeof(string)) {
                    file.SaveAsText((string)value);
                } else {
                    file.SaveAsText(JsonWriter.GetWriter(value).Write(value));
                }
                openChanges = FlushOpenChangesIfNeeded(openChanges + 1);
                return oldVal;
            }
        }

        public async Task<bool> Remove(string key) {
            bool res;
            lock (folderAccessLock) {
                res = GetFile(key).DeleteV2();
                openChanges = FlushOpenChangesIfNeeded(openChanges + 1);
            }
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            lock (folderAccessLock) {
                folderForAllFiles.DeleteV2();
                openChanges = FlushOpenChangesIfNeeded(openChanges + 1);
            }
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        /// <summary> Can be called to manually persist all open changes to the underlying persistence system,
        /// only needs to be called if changes are not persisted instantly already, for example for a
        /// zip based file system this method allows to persist changes e.g. every second </summary>
        public void FlushOpenChangesIfNeeded() {
            if (openChanges == 0) { return; }
            lock (folderAccessLock) {
                openChanges = FlushOpenChangesIfNeeded(openChanges);
            }
        }

        public async Task<bool> ContainsKey(string key) {
            if (GetFile(key).IsNotNullAndExists()) { return true; }
            if (fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return false;
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            IEnumerable<string> fileNames;
            lock (folderAccessLock) {
                if (!folderForAllFiles.Exists) { return Enumerable.Empty<string>(); }
                fileNames = folderForAllFiles.GetFiles().Map(x => x.Name);
            }
            return await fallbackStore.ConcatWithKeys(fileNames.Cached());
        }

    }

}