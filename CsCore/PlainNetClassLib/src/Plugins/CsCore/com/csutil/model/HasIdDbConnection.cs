using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.model {

    public class HasIdDbConnection<T> where T : HasId {

        public IKeyValueStore db;

        public HasIdDbConnection(IKeyValueStore db) { this.db = db; }

        public Task<bool> Contains(T item) { return db.ContainsKey(item.GetId()); }

        public async Task<T> Save(T item) { return (T)(await db.Set(item.GetId(), item)); }

        public Task<T> Load(string key, T defaultValue) { return db.Get<T>(key, defaultValue); }

        public async Task<T[]> LoadAllEntries() {
            T defaultT = default(T);
            var allGetTasks = (await LoadAllIds()).Map(async x => await db.Get(x, defaultT));
            return await Task.WhenAll(allGetTasks);
        }

        public Task<IEnumerable<string>> LoadAllIds() { return db.GetAllKeys(); }

        public Task<bool> Remove(T item) { return db.Remove(item.GetId()); }

        public Task RemoveAll() { return db.RemoveAll(); }

    }

}