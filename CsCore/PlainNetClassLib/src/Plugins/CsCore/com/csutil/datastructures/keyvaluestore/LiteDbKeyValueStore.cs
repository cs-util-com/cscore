using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;

namespace com.csutil.keyvaluestore {

    public class LiteDbKeyValueStore : IKeyValueStore {

        private class PrimitiveWrapper { public object val; }

        private BsonMapper bsonMapper;
        private LiteDatabase db;
        private LiteCollection<BsonDocument> collection;

        public IKeyValueStore fallbackStore { get; set; }

        private static bool IsPrimitiveType(Type t) { return t.IsPrimitive || t == typeof(string); }

        public LiteDbKeyValueStore(FileInfo dbFile) { Init(dbFile); }

        private void Init(FileInfo dbFile, string collectionName = "Default") {
            bsonMapper = new BsonMapper();
            bsonMapper.IncludeFields = true;
            db = new LiteDatabase(dbFile.FullPath(), bsonMapper);
            collection = db.GetCollection(collectionName);
        }

        private BsonDocument GetBson(string key) { return collection.FindById(key); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var bson = GetBson(key);
            if (bson != null) { return (T)InternalGet(bson, typeof(T)); }
            return await fallbackStore.Get(key, defaultValue, (fallbackValue) => InternalSet(key, fallbackValue));
        }

        private object InternalGet(BsonDocument bson, Type targetType) {
            if (IsPrimitiveType(targetType)) { // unwrap the primitive:
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
            if (IsPrimitiveType(objType)) { value = new PrimitiveWrapper() { val = value }; }
            var oldBson = GetBson(key);
            var newVal = bsonMapper.ToDocument(value);
            if (oldBson == null) {
                collection.Insert(key, newVal);
                return null;
            } else {
                var oldVal = InternalGet(oldBson, objType);
                collection.Update(key, newVal);
                return oldVal;
            }
        }

        public async Task<bool> Remove(string key) {
            var res = collection.Delete(key);
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            db.DropCollection(collection.Name);
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
            var key = x.Keys.First();
            AssertV2.AreEqual("_id", key);
            return x[key].AsString;
        }

    }

}