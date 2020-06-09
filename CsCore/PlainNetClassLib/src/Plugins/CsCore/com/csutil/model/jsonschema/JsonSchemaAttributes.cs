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
        /// <summary> Controls whether it's valid to enter any value that is not in the names list, if set to true the 
        /// enum functions as a sort of suggestion list that the user can ignore and enter any other string.
        /// See also https://json-schema.org/understanding-json-schema/reference/array.html#tuple-validation why 
        /// this is set to false by default for enums </summary>
        public bool allowOtherInput = false;

        public EnumAttribute(string description, params string[] names) : base(description) { this.names = names; }
        public EnumAttribute(string description, bool allowOtherInput, params string[] names) : base(description) {
            this.names = names;
            this.allowOtherInput = allowOtherInput;
        }
        public EnumAttribute(string description, Type enumType, bool allowOtherInput = false) : base(description) {
            names = Enum.GetNames(enumType);
            this.allowOtherInput = allowOtherInput;
        }
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class InputLengthAttribute : Attribute {
        /// <summary> If set to 0 will be ingored </summary>
        public int minLength;
        /// <summary> If set to 0 will be ingored </summary>
        public int maxLength;

        public InputLengthAttribute(int max) { maxLength = max; }
        public InputLengthAttribute(int min, int max = 0) { minLength = min; maxLength = max; }
    }

}