using System.Threading.Tasks;

namespace com.csutil.model {

    public static class FeatureFlagExtensions {

        // if the client rnd value is below the rollout % this client gets the feature:
        public static async Task<bool> IsEnabled<T>(this T self) where T : IFeatureFlag {
            AssertV2.AreNotEqual(0, self.localState.randomPercentage, "localState.randomPercentage");
            return self.localState.randomPercentage <= self.rolloutPercentage && await self.IsFeatureUnlocked();
        }

        public static async Task<bool> IsFeatureUnlocked<T>(this T self) where T : IFeatureFlag {
            var tt = "" + typeof(T);
            var key = IoC.inject.GetEventKey<IProgressionSystem<T>>();
            IProgressionSystem<T> progrSys = IoC.inject.Get<IProgressionSystem<T>>(self);
            if (progrSys == null) {
                return true;
            } // No progr. system in place
            return await progrSys.IsFeatureUnlocked(self);
        }

    }

}
