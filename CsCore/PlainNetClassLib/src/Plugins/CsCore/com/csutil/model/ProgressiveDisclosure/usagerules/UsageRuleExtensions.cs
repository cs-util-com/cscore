using System;
using System.Collections.Generic;
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
                        foreach (var rule in self.andRules) {
                            if (rule.isTrue == null) { rule.SetupUsing(analytics); }
                            if (!await rule.isTrue()) { return false; }
                        }
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
            var allFeatureEvents = await analytics.categoryStores[self.featureId].GetAll();
            DateTime lastEvent = allFeatureEvents.Last().GetDateTimeUtc();
            TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
            return lastEventVsNow.Days <= self.days;
        }

        public static async Task<bool> IsFeatureUsedXTimes(this UsageRule self, LocalAnalytics analytics) {
            var allFeatureEvents = await analytics.categoryStores[self.featureId].GetAll();
            var startEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
            return startEvents.Count() >= self.timesUsed.Value;
        }

        public static IEnumerable<IGrouping<DateTime, AppFlowEvent>> GroupByDay(this IEnumerable<AppFlowEvent> self) {
            return self.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
        }

        public static async Task<bool> IsAppUsedXDays(this UsageRule self, LocalAnalytics analytics) {
            var allEvents = await analytics.GetAll();
            return allEvents.GroupByDay().Count() >= self.days;
        }

        public static async Task<bool> IsFeatureUsedXDays(this UsageRule self, LocalAnalytics analytics) {
            var allFeatureEvents = await analytics.categoryStores[self.featureId].GetAll();
            return allFeatureEvents.GroupByDay().Count() >= self.days;
        }

    }

}