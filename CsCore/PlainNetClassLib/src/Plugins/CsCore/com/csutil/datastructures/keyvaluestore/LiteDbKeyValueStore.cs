using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UltraLiteDB;
using Zio;

namespace com.csutil.keyvaluestore {

    public class LiteDbKeyValueStore : IKeyValueStore {

        private class PrimitiveWrapper { public object val; }

        private BsonMapper bsonMapper;
        private Stream dbStream;
        private UltraLiteDatabase db;
        private UltraLiteCollection<BsonDocument> collection;
        private object threadLock = new object();

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }

        public LiteDbKeyValueStore(FileEntry dbFile) { Init(dbFile); }

        private void Init(FileEntry dbFile, string collectionName = "Default") {
            bsonMapper = new BsonMapper();
            bsonMapper.IncludeFields = true;
            dbStream = dbFile.OpenOrCreateForReadWrite();
            db = new UltraLiteDatabase(dbStream, bsonMapper);
            collection = db.GetCollection(collectionName);
        }

        public void Dispose() {
            db.Dispose();
            dbStream.Dispose();
            fallbackStore?.Dispose();
        }

        private BsonDocument GetBson(string key) { return collection.FindById(key); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = this.StartFallbackStoreGetTimer();
            Task<T> fallbackGet = fallbackStore.Get(key, defaultValue, (newVal) => InternalSet(key, newVal));
            await this.WaitLatestFallbackGetTime(s, fallbackGet);

            var bson = GetBson(key);
            if (bson != null) { return (T)InternalGet(bson, typeof(T)); }
            return await fallbackGet;
        }

        private object InternalGet(BsonDocument bson, Type targetType) {
            if (targetType.IsPrimitiveOrSimple()) { // unwrap the primitive:
                return bsonMapper.ToObject<PrimitiveWrapper>(bson).val;
            }
            return bsonMapper.ToObject(targetType, bson);
        }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            var objType = value.GetType();
            if (objType.IsPrimitiveOrSimple()) { value = new PrimitiveWrapper() { val = value }; }
            var oldBson = GetBson(key);
            var newVal = bsonMapper.ToDocument(value);
            if (oldBson == null) {
                lock (threadLock) { collection.Insert(key, newVal); }
                return null;
            } else {
                var oldVal = InternalGet(oldBson, objType);
                lock (threadLock) { collection.Update(key, newVal); }
                return oldVal;
            }
        }

        public async Task<bool> Remove(string key) {
            var res = false;
            lock (threadLock) { res = collection.Delete(key); }
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            lock (threadLock) { db.DropCollection(collection.Name); }
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public async Task<bool> ContainsKey(string key) {
            if (null != GetBson(key)) { return true; }
            if (fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return false;
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            var result = collection.FindAll().Map(x => GetKeyFromBsonDoc(x));
            return await fallbackStore.ConcatAllKeys(result);
        }

        private static string GetKeyFromBsonDoc(BsonDocument x) {
            AssertV2.IsTrue(x.Keys.Any(k => k == "_id"), "No '_id' key found: " + x.Keys.ToStringV2(k => "" + k));
            return x["_id"].AsString;
        }

    }

}