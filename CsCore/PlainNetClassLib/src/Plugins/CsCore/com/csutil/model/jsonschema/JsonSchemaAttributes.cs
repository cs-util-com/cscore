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
    public class RegularExpressionAttribute : Attribute {
        public string Pattern;

        public RegularExpressionAttribute(params string[] pattern) { this.Pattern = RegexUtil.CombineViaAnd(pattern); }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DataTypeV2Attribute : DescriptionAttribute {
        public DataTypeV2 DataType;

        public DataTypeV2Attribute(DataTypeV2 dataType, string description) : base(description) {
            DataType = dataType;
        }
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
    public class RequiredAttribute : Attribute {
        public bool AllowEmptyStrings { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RangeAttribute : Attribute {
        public float? Minimum;
        public float? Maximum;

        public RangeAttribute(float minimum) { Minimum = minimum; }
        public RangeAttribute(float minimum, float maximum) {
            Minimum = minimum; Maximum = maximum;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class StringLengthAttribute : Attribute {
        /// <summary> If set to 0 will be ingored </summary>
        public int MinimumLength;
        /// <summary> If set to 0 will be ingored </summary>
        public int MaximumLength;

        public StringLengthAttribute(int maximumLength) { MaximumLength = maximumLength; }
    }

}