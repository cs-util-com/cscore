using System;
using System.Linq;

namespace com.csutil.model.jsonschema {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute {
        public string description;
        public string defaultVal;

        public DescriptionAttribute(string description) { this.description = description; }
        public DescriptionAttribute(string description, string defaultVal) {
            this.description = description;
            this.defaultVal = defaultVal;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RegexAttribute : Attribute {
        public string regex;

        public RegexAttribute(params string[] regex) { this.regex = RegexUtil.CombineViaAnd(regex); }
        public RegexAttribute(int minChars, params string[] regex) {
            SetRegex(regex, "^.{" + minChars + ",}$");
        }
        public RegexAttribute(int minChars, int maxChars, params string[] regex) {
            SetRegex(regex, "^.{" + minChars + "," + maxChars + "}$");
        }

        private void SetRegex(string[] regex, string s) {
            var minMaxRegex = new string[1] { s };
            this.regex = RegexUtil.CombineViaAnd(minMaxRegex.Union(regex).ToArray());
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ContentAttribute : DescriptionAttribute {
        public ContentFormat type;

        public ContentAttribute(ContentFormat type, string description) : base(description) { this.type = type; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class EnumAttribute : DescriptionAttribute {
        public string[] names;
        /// <summary> Controls whether it's valid to have additional items in the array </summary>
        public bool additionalItems = false;

        public EnumAttribute(string description, params string[] names) : base(description) { this.names = names; }
        public EnumAttribute(string description, Type enumType) : base(description) { names = Enum.GetNames(enumType); }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class MinMaxRangeAttribute : Attribute {
        public float? minimum;
        public float? maximum;

        public MinMaxRangeAttribute(float min) { minimum = min; }
        public MinMaxRangeAttribute(float max, bool exclusiveMaximum = false) { maximum = exclusiveMaximum ? max + float.Epsilon : max; }
        public MinMaxRangeAttribute(float min, float max) { minimum = min; maximum = max; }
    }

}