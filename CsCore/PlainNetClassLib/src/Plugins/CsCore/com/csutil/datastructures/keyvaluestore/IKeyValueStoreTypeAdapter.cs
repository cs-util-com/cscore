using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {
    
    public interface IKeyValueStoreTypeAdapter<T> : IDisposableV2 {
        IKeyValueStore store { get; set; }
        Task<T> Get(string key, T defVal);
        Task<T> Set(string key, T val);
        Task<bool> Remove(string key);
        Task RemoveAll();
        Task<bool> ContainsKey(string key);
        Task<IEnumerable<string>> GetAllKeys();
        Task<IEnumerable<T>> GetAll();
    }
    
}