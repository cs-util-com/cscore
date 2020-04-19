using System.Threading.Tasks;

namespace com.csutil.model {

    public class FeatureFlag {

        public static Task<bool> IsEnabled(string featureId) {
            return FeatureFlagManager.instance.IsEnabled(featureId);
        }

        public string id;
        public int rolloutPercentage;
        public int requiredXp;
        public LocalState localState = new LocalState();

        // if the client rnd value is below the rollout % this client gets the feature:
        public async Task<bool> IsEnabled() {
            AssertV2.AreNotEqual(0, localState.randomPercentage, "localState.randomPercentage");
            return localState.randomPercentage <= rolloutPercentage && await IsFeatureUnlocked();
        }

        public async Task<bool> IsFeatureUnlocked() {
            ProgressionSystem progrSys = IoC.inject.Get<ProgressionSystem>(this);
            if (progrSys == null) { return true; } // No progr. system in place
            return await progrSys.IsFeatureUnlocked(this);
        }

        public class LocalState {
            public int randomPercentage;
        }

    }

}