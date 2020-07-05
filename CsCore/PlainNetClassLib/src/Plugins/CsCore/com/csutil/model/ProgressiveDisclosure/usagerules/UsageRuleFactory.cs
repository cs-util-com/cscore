using com.csutil.logging.analytics;

namespace com.csutil.model.usagerules {

    public static class UsageRuleFactory {

        public static UsageRule NewConcatRule(this LocalAnalytics self, params UsageRule[] andRules) {
            var featureNotUsedAnymoreRule = new UsageRule(UsageRule.ConcatRule) { andRules = andRules };
            featureNotUsedAnymoreRule.SetupUsing(self);
            return featureNotUsedAnymoreRule;
        }


        public static UsageRule NewFeatureUsedInTheLastXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureUsedInTheLastXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }

        public static UsageRule NewAppNotUsedInTheLastXDaysRule(this LocalAnalytics analytics, int days) {
            var self = new UsageRule(UsageRule.AppNotUsedInTheLastXDays) { days = days };
            self.SetupUsing(analytics);
            return self;
        }


        public static UsageRule NewFeatureUsedXTimesRule(this LocalAnalytics self, string featureId, int times) {
            return new UsageRule(UsageRule.FeatureUsedXTimes) { featureId = featureId, timesUsed = times }.SetupUsing(self);
        }

        public static UsageRule NewFeatureNotUsedXTimesRule(this LocalAnalytics self, string featureId, int times) {
            return new UsageRule(UsageRule.FeatureNotUsedXTimes) { featureId = featureId, timesUsed = times }.SetupUsing(self);
        }


        public static UsageRule NewAppUsedInTheLastXDaysRule(this LocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppUsedInTheLastXDays) { days = days }.SetupUsing(self);
        }

        public static UsageRule NewFeatureNotUsedInTheLastXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureNotUsedInTheLastXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }


        public static UsageRule NewAppUsedXDaysRule(this LocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppUsedXDays) { days = days }.SetupUsing(self);
        }

        public static UsageRule NewAppNotUsedXDaysRule(this LocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppNotUsedXDays) { days = days }.SetupUsing(self);
        }


        public static UsageRule NewFeatureUsedXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureUsedXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }

        public static UsageRule NewFeatureNotUsedXDaysRule(this LocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureNotUsedXDays) { featureId = featureId, days = days }.SetupUsing(self);
        }

    }

}
