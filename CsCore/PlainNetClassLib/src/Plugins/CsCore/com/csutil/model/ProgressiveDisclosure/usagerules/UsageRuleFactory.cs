using System.Linq;
using com.csutil.logging.analytics;

namespace com.csutil.model.usagerules {

    public static class UsageRuleFactory {

        public static UsageRule NewConcatRule(this ILocalAnalytics self, params UsageRule[] andRules) {
            return new UsageRule(UsageRule.ConcatRule) { andRules = andRules.ToList() }.SetupUsing(self);
        }


        public static UsageRule NewFeatureUsedInTheLastXDaysRule(this ILocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureUsedInTheLastXDays) { categoryId = featureId, days = days }.SetupUsing(self);
        }

        public static UsageRule NewAppNotUsedInTheLastXDaysRule(this ILocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppNotUsedInTheLastXDays) { days = days }.SetupUsing(self);
        }


        public static UsageRule NewFeatureUsedXTimesRule(this ILocalAnalytics self, string featureId, int times) {
            return new UsageRule(UsageRule.FeatureUsedXTimes) { categoryId = featureId, timesUsed = times }.SetupUsing(self);
        }

        public static UsageRule NewFeatureNotUsedXTimesRule(this ILocalAnalytics self, string featureId, int times) {
            return new UsageRule(UsageRule.FeatureNotUsedXTimes) { categoryId = featureId, timesUsed = times }.SetupUsing(self);
        }


        public static UsageRule NewAppUsedInTheLastXDaysRule(this ILocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppUsedInTheLastXDays) { days = days }.SetupUsing(self);
        }

        public static UsageRule NewFeatureNotUsedInTheLastXDaysRule(this ILocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureNotUsedInTheLastXDays) { categoryId = featureId, days = days }.SetupUsing(self);
        }


        public static UsageRule NewAppUsedXDaysRule(this ILocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppUsedXDays) { days = days }.SetupUsing(self);
        }

        public static UsageRule NewAppNotUsedXDaysRule(this ILocalAnalytics self, int days) {
            return new UsageRule(UsageRule.AppNotUsedXDays) { days = days }.SetupUsing(self);
        }

        public static UsageRule NewNotificationMinXDaysOldRule(this ILocalAnalytics self, string notificationId, int days) {
            return new UsageRule(UsageRule.NotificationMinXDaysOld) { categoryId = notificationId, days = days }.SetupUsing(self);
        }

        public static UsageRule NewFeatureUsedXDaysRule(this ILocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureUsedXDays) { categoryId = featureId, days = days }.SetupUsing(self);
        }

        public static UsageRule NewFeatureNotUsedXDaysRule(this ILocalAnalytics self, string featureId, int days) {
            return new UsageRule(UsageRule.FeatureNotUsedXDays) { categoryId = featureId, days = days }.SetupUsing(self);
        }

    }

}
