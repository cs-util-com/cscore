using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public interface IKeyValueStore {
        Task<T> Get<T>(string key, T defaultValue);
        Task<object> Set(string key, object value);
        Task<bool> Remove(string key);
        Task RemoveAll();
        Task<bool> ContainsKey(string key);
        Task<IEnumerable<string>> GetAllKeys();
        IKeyValueStore fallbackStore { get; set; }
    }

}