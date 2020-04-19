using System.Threading.Tasks;

namespace com.csutil.model {

    public interface ProgressionSystem {

        Task<bool> IsFeatureUnlocked(FeatureFlag featureFlag);

    }

}