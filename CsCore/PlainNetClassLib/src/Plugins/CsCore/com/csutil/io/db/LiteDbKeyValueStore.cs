using System.IO;
using System.Threading.Tasks;
using LiteDB;

namespace com.csutil.keyvaluestore {

    public class LiteDbKeyValueStore : IKeyValueStore {
        private BsonMapper bsonMapper;
        private LiteDatabase db;
        private LiteCollection<BsonDocument> collection;
        private IKeyValueStore fallbackStore;

        public LiteDbKeyValueStore(FileInfo dbFile) { Init(dbFile); }

        private void Init(System.IO.FileInfo dbFile, string collectionName = "Default") {
            bsonMapper = new BsonMapper();
            bsonMapper.IncludeFields = true;
            db = new LiteDatabase(dbFile.FullPath(), bsonMapper);
            collection = db.GetCollection(collectionName);
        }

        public async Task<bool> ContainsKey(string key) {
            if (null != GetBson(key)) { return true; }
            if (fallbackStore != null) return await fallbackStore.ContainsKey(key);
            return false;
        }

        private BsonDocument GetBson(string key) { return collection.FindById(key); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var bson = GetBson(key);
            if (bson != null) { return (T)bsonMapper.ToObject(typeof(T), bson); }
            return defaultValue;
        }

        public async Task<bool> Remove(string key) {
            return collection.Delete(key);
        }

        public async Task RemoveAll() {
            db.DropCollection(collection.Name);
        }

        public async Task<object> Set(string key, object obj) {
            var oldVal = GetBson(key);
            var newVal = bsonMapper.ToDocument(obj);
            if (oldVal == null) {
                collection.Insert(key, newVal);
                return null;
            } else {
                collection.Update(key, newVal);
                return bsonMapper.ToObject(obj.GetType(), oldVal);
            }
        }

        public void SetFallbackStore(IKeyValueStore fallbackStore) { this.fallbackStore = fallbackStore; }

    }

}