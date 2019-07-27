using System.Threading.Tasks;

namespace com.csutil.io.keyvaluestore {

    public interface IKeyValueStore {

        void SetFallbackStore(IKeyValueStore fallbackStore);

        Task<T> Get<T>(string key, T defaultValue);

        Task Set(string key, object obj);

    }

}