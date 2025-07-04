using System;
using System.ComponentModel.DataAnnotations;

namespace com.csutil.model.jsonschema {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DescriptionAttribute : System.ComponentModel.DescriptionAttribute {
        public string defaultVal { get; }
        [Obsolete("Use 'Description' property instead")]
        public string description => Description; // old field kept alive as read-only wrapper

        public DescriptionAttribute(string description) : base(description) { }
        public DescriptionAttribute(string description, string defaultVal)
            : base(description) => this.defaultVal = defaultVal;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RegexAttribute : RegularExpressionAttribute {
        [Obsolete("Use 'Pattern' property instead")]
        public string regex => Pattern; // compat shim
        public RegexAttribute(params string[] patterns)
            : base(RegexUtil.CombineViaAnd(patterns)) {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ContentAttribute : DescriptionAttribute {
        public ContentFormat type { get; }
        public ContentAttribute(ContentFormat type, string description)
            : base(description) => this.type = type;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class EnumAttribute : DescriptionAttribute {
        public string[] names { get; }
        public bool allowOtherInput { get; set; }

        public EnumAttribute(string description, params string[] names)
            : this(description, false, names) {
        }

        public EnumAttribute(string description, bool allowOtherInput, params string[] names)
            : base(description) {
            this.names = names;
            this.allowOtherInput = allowOtherInput;
        }

        public EnumAttribute(string description, Type enumType, bool allowOtherInput = false)
            : base(description) {
            names = Enum.GetNames(enumType);
            this.allowOtherInput = allowOtherInput;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class MinMaxRangeAttribute : RangeAttribute {

        public MinMaxRangeAttribute(float min) : base(min, float.MaxValue) { }

        public MinMaxRangeAttribute(float max, bool exclusiveMaximum = false)
            : base(float.MinValue, exclusiveMaximum ? max - float.Epsilon : max) {
        }

        public MinMaxRangeAttribute(float min, float max) : base(min, max) { }

        [Obsolete("Use 'Minimum' property (casted to float or double) instead")]
        public float? minimum => (float?)Minimum;
        [Obsolete("Use 'Maximum' property (casted to float or double) instead")]
        public float? maximum => (float?)Maximum;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class InputLengthAttribute : StringLengthAttribute {

        public InputLengthAttribute(int max) : base(max) { }

        public InputLengthAttribute(int min, int max = 0)
            : base(max == 0 ? int.MaxValue : max) {
            MinimumLength = min;
        }

        [Obsolete("Use 'MinimumLength' property instead")]
        public int minLength => MinimumLength;
        [Obsolete("Use 'MaximumLength' property instead")]
        public int maxLength => MaximumLength;
    }

    /// <summary> A class must have 2 of these annotations per dropdownId, one on a string[] field (For the dropdown options) and
    /// one on an int field (for the selected index in the dropdown) </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DropDownAttribute : Attribute {
        public string dropdownId { get; set; }
        /// <summary> [DropDown(dropdownId: "MyDropdown1")] </summary>
        public DropDownAttribute() { }
        /// <summary> [DropDown("MyDropdown1")] </summary>
        public DropDownAttribute(string dropdownId) => this.dropdownId = dropdownId;
    }
}

namespace System.ComponentModel.DataAnnotations {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class MaxWordsAttribute : com.csutil.model.jsonschema.RegexAttribute {
        public MaxWordsAttribute(int maxWords) : base($"^(?:\\S+\\s+){{0,{maxWords - 1}}}\\S+$") { }
    }

}