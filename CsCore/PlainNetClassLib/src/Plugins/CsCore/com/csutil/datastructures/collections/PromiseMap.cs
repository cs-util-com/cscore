using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace com.csutil {

    /// <summary> The PromiseMap represents a caching pattern where instead of having a key:value cache instead key:promise-that-resolves-to-value—doing is used, this gives you
    /// dog piling prevention for free, because the first lookup of a value triggers the computation to fetch it while subsequent lookups wait on the same promise to
    /// resolve—or resolve instantly if the computation has completed. See https://news.ycombinator.com/item?id=32189416 for further discussion of this pattern </summary>
    public class PromiseMap<TKey, TValue> : ConcurrentDictionary<TKey, Task<TValue>> {

    }

}