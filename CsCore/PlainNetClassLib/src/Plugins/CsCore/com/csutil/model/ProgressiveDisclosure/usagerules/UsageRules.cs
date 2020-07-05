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

    public class AppDaysRule : AppRule {
        public int? days;
    }

    public class FeatureDaysRule : AppRule {
        public string featureId;
        public int? days;
    }

    public class FeatureCounterRule : AppRule {
        public string featureId;
        public int? timesUsed;
    }

}
