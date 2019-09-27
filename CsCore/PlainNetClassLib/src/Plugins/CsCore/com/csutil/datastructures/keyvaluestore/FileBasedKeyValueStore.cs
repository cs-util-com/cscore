using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class FileBasedKeyValueStore : IKeyValueStore {

        private DirectoryInfo folderForAllFiles;
        public IKeyValueStore fallbackStore { get; set; }

        private class PrimitiveWrapper { public object val; }
        private static bool IsPrimitiveType(Type t) { return t.IsPrimitive; }

        public FileBasedKeyValueStore(DirectoryInfo folderForAllFiles) { this.folderForAllFiles = folderForAllFiles; }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var fileForKey = GetFile(key);
            if (fileForKey.ExistsV2()) { return (T)InternalGet(fileForKey, typeof(T)); }
            return await fallbackStore.Get(key, defaultValue, (fallbackValue) => InternalSet(key, fallbackValue));
        }

        private object InternalGet(FileInfo fileForKey, Type type) {
            if (IsPrimitiveType(type)) { return fileForKey.LoadAs<PrimitiveWrapper>().val; }
            return fileForKey.LoadAs(type);
        }

        public FileInfo GetFile(string key) { return folderForAllFiles.GetChild(key); }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            var objType = value.GetType();
            if (IsPrimitiveType(objType)) { value = new PrimitiveWrapper() { val = value }; }
            var file = GetFile(key);
            var oldVal = file.IsNotNullAndExists() ? InternalGet(file, objType) : null;
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
            var result = folderForAllFiles.GetFiles().Map(x => x.Name);
            return await fallbackStore.ConcatAllKeys(result);
        }

    }

}