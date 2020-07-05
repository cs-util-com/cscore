using System;
using System.Threading.Tasks;

namespace com.csutil.model.usagerules {

    public abstract class AppRule {
        public Func<Task<bool>> isTrue { get; set; }
    }

    public class RuleConcat : AppRule {
        public AppRule[] andRules;
        public RuleConcat(params AppRule[] andRules) {
            this.andRules = andRules;
            isTrue = async () => {
                foreach (var rule in andRules) { if (!await rule.isTrue()) { return false; } }
                return true;
            };
        }
    }

    public abstract class AppDays : AppRule {
        public int? days;

        public static TimeSpan GetTimeSpanSinceLatestLaunch() {
            return DateTimeV2.UtcNow - EnvironmentV2.instance.systemInfo.GetLatestLaunchDate();
        }
    }

    public abstract class FeatureDays : AppRule {
        public string featureId;
        public int? days;
    }

    public abstract class FeatureTimes : AppRule {
        public string featureId;
        public int? timesUsed;
    }

    public class AppUsedXDays : AppDays { }

    public class AppNotUsedXDays : AppDays { }

    public class FeatureUsedXDays : FeatureDays { }

    public class FeatureNotUsedXDays : FeatureDays { }

    public class AppUsedInTheLastXDays : AppDays {
        public AppUsedInTheLastXDays() {
            isTrue = () => Task.FromResult(GetTimeSpanSinceLatestLaunch().Days < days);
        }
    }

    public class AppNotUsedInTheLastXDays : AppDays {
        public AppNotUsedInTheLastXDays() {
            isTrue = () => Task.FromResult(GetTimeSpanSinceLatestLaunch().Days > days);
        }
    }

    public class FeatureUsedInTheLastXDays : FeatureDays { }

    public class FeatureNotUsedInTheLastXDays : FeatureDays { }

    public class FeatureUsedXTimes : FeatureTimes { }

    public class FeatureNotUsedXTimes : FeatureTimes { }

}
