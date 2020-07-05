using System;
using System.Threading.Tasks;

namespace com.csutil.model.usagerules {

    public class UsageRule {

        public const string ConcatRule = "ConcatRule";

        public const string AppUsedXDays = "AppUsedXDays";
        public const string AppUsedInTheLastXDays = "AppUsedInTheLastXDays";
        public const string FeatureUsedXDays = "FeatureUsedXDays";
        public const string FeatureUsedXTimes = "FeatureUsedXTimes";
        public const string FeatureUsedInTheLastXDays = "FeatureUsedInTheLastXDays";

        public const string AppNotUsedXDays = "ApNotpUsedXDays";
        public const string AppNotUsedInTheLastXDays = "AppNotUsedInTheLastXDays";
        public const string FeatureNotUsedXDays = "FeatureNotUsedXDays";
        public const string FeatureNotUsedXTimes = "FeatureNotUsedXTimes";
        public const string FeatureNotUsedInTheLastXDays = "FeatureNotUsedInTheLastXDays";

        public string ruleType;
        public string featureId;
        public int? days;
        public int? timesUsed;
        public UsageRule[] andRules;

        public UsageRule(string ruleType) { this.ruleType = ruleType; }

        public Func<Task<bool>> isTrue { get; set; }

    }

}