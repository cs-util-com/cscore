using System.Threading.Tasks;
using LiteDB;

namespace com.csutil.keyvaluestore {

    public class LiteDbKeyValueStore : IKeyValueStore {
        private BsonMapper bsonMapper;
        private LiteDatabase db;
        private LiteCollection<BsonDocument> collection;
        private string collectionName;

        public LiteDbKeyValueStore() {
            collectionName = "Test1";
            var testFolder = EnvironmentV2.instance.GetOrAddTempFolder("Test");
            var testFile = testFolder.GetChild("LiteDbKeyValueStoreTest1.db");
            bsonMapper = new BsonMapper();
            db = new LiteDatabase(testFile.FullPath());
            collection = db.GetCollection(collectionName);
        }

        public async Task<bool> ContainsKey(string key) {
            return null != GetBson(key);
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
            db.DropCollection(collectionName);
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

        public void SetFallbackStore(IKeyValueStore fallbackStore) {
            throw new System.NotImplementedException();
        }

    }

}