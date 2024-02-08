using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.model {

    public interface IProgressionSystem<T> : IDisposableV2 where T : IFeatureFlag {

        Task<bool> IsFeatureUnlocked(T featureFlag);
        Task<IEnumerable<T>> GetLockedFeatures();
        Task<IEnumerable<T>> GetUnlockedFeatures();
        Task<int> GetLatestXp();

    }

}