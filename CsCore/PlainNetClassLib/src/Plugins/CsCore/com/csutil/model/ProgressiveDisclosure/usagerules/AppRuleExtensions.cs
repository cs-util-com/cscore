using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.logging.analytics;

namespace com.csutil.model.usagerules {

    public static class AppRuleExtensions {

        public static AppRule NewConcatRule(this LocalAnalytics self, params AppRule[] andRules) {
            var featureNotUsedAnymoreRule = new AppRule(AppRule.ConcatRule) { andRules = andRules };
            featureNotUsedAnymoreRule.SetupUsing(self);
            return featureNotUsedAnymoreRule;
        }

        public static AppRule NewFeatureUsedInTheLastXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new AppRule(AppRule.FeatureUsedInTheLastXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }
        public static AppRule NewAppNotUsedInTheLastXDaysRule(this LocalAnalytics analytics, int days) {
            var self = new AppRule(AppRule.AppNotUsedInTheLastXDays) { days = days };
            self.SetupUsing(analytics);
            return self;
        }

        public static AppRule NewFeatureUsedXTimesRule(this LocalAnalytics self, string featureId, int times) {
            return new AppRule(AppRule.FeatureUsedXTimes) { featureId = featureId, timesUsed = times }.SetupUsing(self);
        }
        public static AppRule NewFeatureNotUsedXTimesRule(this LocalAnalytics self, string featureId, int times) {
            return new AppRule(AppRule.FeatureNotUsedXTimes) { featureId = featureId, timesUsed = times }.SetupUsing(self);
        }

        public static AppRule NewAppUsedInTheLastXDaysRule(this LocalAnalytics self, int days) {
            return new AppRule(AppRule.AppUsedInTheLastXDays) { days = days }.SetupUsing(self);
        }
        public static AppRule NewFeatureNotUsedInTheLastXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new AppRule(AppRule.FeatureNotUsedInTheLastXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }

        public static AppRule NewAppUsedXDaysRule(this LocalAnalytics self, int days) {
            return new AppRule(AppRule.AppUsedXDays) { days = days }.SetupUsing(self);
        }
        public static AppRule NewAppNotUsedXDaysRule(this LocalAnalytics self, int days) {
            return new AppRule(AppRule.AppNotUsedXDays) { days = days }.SetupUsing(self);
        }

        public static AppRule NewFeatureUsedXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new AppRule(AppRule.FeatureUsedXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }
        public static AppRule NewFeatureNotUsedXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new AppRule(AppRule.FeatureNotUsedXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }

        public static AppRule SetupUsing(this AppRule self, LocalAnalytics analytics) {
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

                    case AppRule.ConcatRule:
                        foreach (var rule in self.andRules) { if (!await rule.isTrue()) { return false; } }
                        return true;

                    default:
                        Log.e("Unknown ruleType: " + self.ruleType);
                        return false;
                }
            };
            return self;
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