using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;

namespace com.csutil.model.usagerules {

    public static class UsageRuleExtensions {

        public static UsageRule SetupUsing(this UsageRule self, LocalAnalytics analytics) {
            self.isTrue = async () => {
                switch (self.ruleType) {

                    case UsageRule.AppUsedXDays: return await self.IsAppUsedXDays(analytics);
                    case UsageRule.AppNotUsedXDays: return !await self.IsAppUsedXDays(analytics);

                    case UsageRule.AppUsedInTheLastXDays: return self.IsAppUsedInTheLastXDays();
                    case UsageRule.AppNotUsedInTheLastXDays: return !self.IsAppUsedInTheLastXDays();

                    case UsageRule.FeatureUsedInTheLastXDays: return await self.IsFeatureUsedInTheLastXDays(analytics);
                    case UsageRule.FeatureNotUsedInTheLastXDays: return !await self.IsFeatureUsedInTheLastXDays(analytics);

                    case UsageRule.FeatureUsedXDays: return await self.IsFeatureUsedXDays(analytics);
                    case UsageRule.FeatureNotUsedXDays: return !await self.IsFeatureUsedXDays(analytics);

                    case UsageRule.FeatureUsedXTimes: return await self.IsFeatureUsedXTimes(analytics);
                    case UsageRule.FeatureNotUsedXTimes: return !await self.IsFeatureUsedXTimes(analytics);

                    case UsageRule.NotificationMinXDaysOld: return await self.IsNotificationMinXDaysOld(analytics);

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
            return tSinceLatestLaunch.TotalDays < self.days;
        }

        public static async Task<bool> IsFeatureUsedInTheLastXDays(this UsageRule self, LocalAnalytics analytics) {
            var allEvents = await analytics.GetAllEventsForCategory(self.categoryId);
            if (allEvents.IsNullOrEmpty()) { return false; }
            DateTime lastEvent = allEvents.Last().GetDateTimeUtc();
            TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
            return lastEventVsNow.TotalDays <= self.days;
        }

        public static async Task<bool> IsFeatureUsedXTimes(this UsageRule self, LocalAnalytics analytics) {
            var startEvents = await analytics.GetStartEvents(self.categoryId);
            return startEvents.CountIsAbove(self.timesUsed.Value - 1);
        }

        public static async Task<IEnumerable<AppFlowEvent>> GetStartEvents(this LocalAnalytics self, string categoryId) {
            var allEvents = await self.GetAllEventsForCategory(categoryId);
            return allEvents.Filter(x => x.action == EventConsts.START);
        }

        public static IEnumerable<IGrouping<DateTime, AppFlowEvent>> GroupByDay(this IEnumerable<AppFlowEvent> self) {
            return self.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
        }

        public static async Task<bool> IsAppUsedXDays(this UsageRule self, LocalAnalytics analytics) {
            var allEvents = await analytics.GetAll();
            return allEvents.GroupByDay().CountIsAbove(self.days.Value - 1);
        }

        public static async Task<bool> IsFeatureUsedXDays(this UsageRule self, LocalAnalytics analytics) {
            var allEvents = await analytics.GetAllEventsForCategory(self.categoryId);
            return allEvents.GroupByDay().CountIsAbove(self.days.Value - 1);
        }

        public static async Task<bool> IsNotificationMinXDaysOld(this UsageRule self, LocalAnalytics analytics) {
            var allEvents = await analytics.GetAllEventsForCategory(EventConsts.catUsage);
            var showEvents = allEvents.Filter(x => x.action == EventConsts.SHOW + "_" + self.categoryId);
            if (showEvents.IsNullOrEmpty()) { return false; }
            DateTime firstShownEvent = showEvents.Last().GetDateTimeUtc();
            TimeSpan firstShownVsNow = DateTimeV2.UtcNow - firstShownEvent;
            return firstShownVsNow.TotalDays >= self.days;
        }

        private static async Task<IEnumerable<AppFlowEvent>> GetAllEventsForCategory(this LocalAnalytics self, string categoryId) {
            if (!self.categoryStores.ContainsKey(categoryId)) { return Enumerable.Empty<AppFlowEvent>(); }
            return await self.categoryStores[categoryId].GetAll();
        }

        public static async Task<IEnumerable<UsageRule>> GetRulesInitialized(this KeyValueStoreTypeAdapter<UsageRule> self, LocalAnalytics analytics) {
            var rules = await self.GetAll();
            foreach (var rule in rules) {
                if (!rule.concatRuleIds.IsNullOrEmpty()) {
                    rule.andRules = new List<UsageRule>();
                    foreach (var id in rule.concatRuleIds) {
                        rule.andRules.Add(await self.Get(id, null));
                    }
                }
                if (rule.isTrue == null) { rule.SetupUsing(analytics); }
            }
            return rules;
        }

    }

}