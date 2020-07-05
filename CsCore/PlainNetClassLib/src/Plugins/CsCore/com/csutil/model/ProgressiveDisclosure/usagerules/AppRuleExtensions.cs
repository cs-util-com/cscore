using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.logging.analytics;

namespace com.csutil.model.usagerules {
    public static class AppRuleExtensions {

        public static void SetupUsing(this AppRule self, LocalAnalytics analytics) {
            self.isTrue = async () => {
                switch (self.ruleType) {

                    case AppRule.AppUsedXDays: return await self.IsAppUsedXDays(analytics);
                    case AppRule.AppUsedInTheLastXDays: return self.IsAppUsedInTheLastXDays();

                    case AppRule.AppNotUsedXDays: return !await self.IsAppUsedXDays(analytics);
                    case AppRule.AppNotUsedInTheLastXDays: return !self.IsAppUsedInTheLastXDays();

                    case AppRule.FeatureUsedInTheLastXDays: return await self.IsFeatureUsedInTheLastXDays(analytics);
                    case AppRule.FeatureUsedXDays: return await self.IsFeatureUsedXDays(analytics);
                    case AppRule.FeatureUsedXTimes: return await self.IsFeatureUsedXTimes(analytics);

                    case AppRule.FeatureNotUsedInTheLastXDays: return !await self.IsFeatureUsedInTheLastXDays(analytics);
                    case AppRule.FeatureNotUsedXDays: return !await self.IsFeatureUsedXDays(analytics);
                    case AppRule.FeatureNotUsedXTimes: return !await self.IsFeatureUsedXTimes(analytics);

                    default:
                        Log.e("Unknown ruleType: " + self.ruleType);
                        return false;
                }
            };
        }

        public static bool IsAppUsedInTheLastXDays(this AppRule self) {
            var tSinceLatestLaunch = DateTimeV2.UtcNow - EnvironmentV2.instance.systemInfo.GetLatestLaunchDate();
            return tSinceLatestLaunch.Days < self.days;
        }

        public static async Task<bool> IsFeatureUsedInTheLastXDays(this AppRule self, LocalAnalytics analytics) {
            var featureEventStore = analytics.categoryStores[self.featureId];
            DateTime lastEvent = (await featureEventStore.GetAll()).Last().GetDateTimeUtc();
            TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
            return lastEventVsNow.Days <= self.days;
        }

        public static async Task<bool> IsFeatureUsedXTimes(this AppRule self, LocalAnalytics analytics) {
            var featureEventStore = analytics.categoryStores[self.featureId];
            var allFeatureEvents = await featureEventStore.GetAll();
            var startEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
            return startEvents.Count() >= self.timesUsed.Value;
        }

        public static async Task<bool> IsAppUsedXDays(this AppRule self, LocalAnalytics analytics) {
            var allAppEvents = await analytics.GetAll();
            var dayGroups = allAppEvents.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
            return dayGroups.Count() >= self.days;
        }

        public static async Task<bool> IsFeatureUsedXDays(this AppRule self, LocalAnalytics analytics) {
            var all = await analytics.categoryStores[self.featureId].GetAll();
            var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
            return dayGroups.Count() >= self.days;
        }

    }

}