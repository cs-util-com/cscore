using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public interface IKeyValueStore {
        Task<bool> ContainsKey(string key);
        Task<T> Get<T>(string key, T defaultValue);
        Task<object> Set(string key, object obj);
        Task<bool> Remove(string key);
        Task RemoveAll();
        void SetFallbackStore(IKeyValueStore fallbackStore);
        Task<IEnumerable<string>> GetAllKeys();
    }

}