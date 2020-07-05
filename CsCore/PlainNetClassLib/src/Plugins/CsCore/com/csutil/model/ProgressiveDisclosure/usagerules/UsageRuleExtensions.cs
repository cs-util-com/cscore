using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.logging.analytics;

namespace com.csutil.model.usagerules {

    public static class UsageRuleExtensions {

        public static UsageRule SetupUsing(this UsageRule self, LocalAnalytics analytics) {
            self.isTrue = async () => {
                switch (self.ruleType) {

                    case UsageRule.AppUsedXDays: return await self.IsAppUsedXDays(analytics);
                    case UsageRule.AppUsedInTheLastXDays: return self.IsAppUsedInTheLastXDays();

                    case UsageRule.AppNotUsedXDays: return !await self.IsAppUsedXDays(analytics);
                    case UsageRule.AppNotUsedInTheLastXDays: return !self.IsAppUsedInTheLastXDays();

                    case UsageRule.FeatureUsedInTheLastXDays: return await self.IsFeatureUsedInTheLastXDays(analytics);
                    case UsageRule.FeatureUsedXDays: return await self.IsFeatureUsedXDays(analytics);
                    case UsageRule.FeatureUsedXTimes: return await self.IsFeatureUsedXTimes(analytics);

                    case UsageRule.FeatureNotUsedInTheLastXDays: return !await self.IsFeatureUsedInTheLastXDays(analytics);
                    case UsageRule.FeatureNotUsedXDays: return !await self.IsFeatureUsedXDays(analytics);
                    case UsageRule.FeatureNotUsedXTimes: return !await self.IsFeatureUsedXTimes(analytics);

                    case UsageRule.ConcatRule:
                        foreach (var rule in self.andRules) { if (!await rule.isTrue()) { return false; } }
                        return true;

                    default:
                        Log.e("Unknown ruleType: " + self.ruleType);
                        return false;
                }
            };
            return self;
        }

        public static bool IsAppUsedInTheLastXDays(this UsageRule self) {
            var tSinceLatestLaunch = DateTimeV2.UtcNow - EnvironmentV2.instance.systemInfo.GetLatestLaunchDate();
            return tSinceLatestLaunch.Days < self.days;
        }

        public static async Task<bool> IsFeatureUsedInTheLastXDays(this UsageRule self, LocalAnalytics analytics) {
            var featureEventStore = analytics.categoryStores[self.featureId];
            DateTime lastEvent = (await featureEventStore.GetAll()).Last().GetDateTimeUtc();
            TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
            return lastEventVsNow.Days <= self.days;
        }

        public static async Task<bool> IsFeatureUsedXTimes(this UsageRule self, LocalAnalytics analytics) {
            var featureEventStore = analytics.categoryStores[self.featureId];
            var allFeatureEvents = await featureEventStore.GetAll();
            var startEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
            return startEvents.Count() >= self.timesUsed.Value;
        }

        public static async Task<bool> IsAppUsedXDays(this UsageRule self, LocalAnalytics analytics) {
            var allAppEvents = await analytics.GetAll();
            var dayGroups = allAppEvents.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
            return dayGroups.Count() >= self.days;
        }

        public static async Task<bool> IsFeatureUsedXDays(this UsageRule self, LocalAnalytics analytics) {
            var all = await analytics.categoryStores[self.featureId].GetAll();
            var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
            return dayGroups.Count() >= self.days;
        }

    }

}