using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace com.csutil.model.jsonschema {

    [Obsolete("Use '[System.ComponentModel.Description]' instead")]
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : System.ComponentModel.DescriptionAttribute {
        public string defaultVal { get; }
        [Obsolete("Use 'Description' property instead")]
        public string description => Description; // old field kept alive as read-only wrapper

        public DescriptionAttribute(string description) : base(description) { }
        public DescriptionAttribute(string description, string defaultVal)
            : base(description) => this.defaultVal = defaultVal;
    }

    [Obsolete("Use '[RegularExpression(...)]' instead")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RegexAttribute : RegularExpressionAttribute {
        [Obsolete("Use 'Pattern' property instead")]
        public string regex => Pattern; // compat shim
        public RegexAttribute(params string[] patterns)
            : base(RegexUtil.CombineViaAnd(patterns)) {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ContentAttribute : System.ComponentModel.DescriptionAttribute {
        public ContentFormat type { get; }
        public ContentAttribute(ContentFormat type, string description)
            : base(description) => this.type = type;
    }

    [AttributeUsage(System.AttributeTargets.All)]
    public class EnumAttribute : System.ComponentModel.DescriptionAttribute {
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

    [Obsolete("Use '[System.ComponentModel.DataAnnotations.Required]' instead")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute {
    }

    [Obsolete("Use '[System.ComponentModel.DataAnnotations.Range]' instead")]
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

    [Obsolete("Use '[System.ComponentModel.DataAnnotations.StringLength]' instead")]
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

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MaxWordsAttribute : RegularExpressionAttribute {
        public MaxWordsAttribute(int maxWords) : base($"^(?:\\S+\\s+){{0,{maxWords - 1}}}\\S+$") { }
    }

    /// <summary>
    /// Recursively validates the value of a property or field by invoking the
    /// <see cref="System.ComponentModel.DataAnnotations.Validator"/> pipeline on the
    /// referenced object-graph.  When applied to a collection (any <c>IEnumerable</c>
    /// except <c>string</c>) the attribute optionally validates every element in the
    /// collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ValidateObjectAttribute : ValidationAttribute {
        /// <summary>
        /// When the target value implements IEnumerable (and is not a string) every element is validated.
        /// </summary>
        public bool ValidateCollectionElements { get; set; } = true;

        public override bool RequiresValidationContext => true; // recommended by MS docs 

        protected override ValidationResult IsValid(object value, ValidationContext context) {
            if (value is null) return ValidationResult.Success; // let [Required] handle nulls

            // ---------- 1) Figure out what we need to validate -------------------------------
            IEnumerable<object> items = (value is IEnumerable e) && !(value is string) && ValidateCollectionElements
                ? e.Cast<object>().Where(o => o != null)
                : new[] { value };

            var allErrors = new List<ValidationResult>();

            foreach (var item in items) {
                // ---------- 2) First run the default validator (properties only) --------------
                var tmpResults = new List<ValidationResult>();
                var tmpContext = new ValidationContext(item, context, context.Items);
                Validator.TryValidateObject(item, tmpContext, tmpResults, validateAllProperties: true); 

                // ---------- 3) Add explicit *field* validation --------------------------------
                foreach (var field in item.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)) {
                    var attrs = field.GetCustomAttributes<ValidationAttribute>(true).ToArray();
                    if (attrs.Length == 0) continue;

                    var fieldValue = field.GetValue(item);
                    var fieldCtx = new ValidationContext(item, context, context.Items)
                        { MemberName = field.Name };

                    foreach (var attr in attrs) {
                        var res = attr.GetValidationResult(fieldValue, fieldCtx);
                        if (res != ValidationResult.Success) tmpResults.Add(res);
                    }
                }

                // ---------- 4) Prefix nested member names with the current property / field ---
                foreach (var r in tmpResults) {
                    var prefix = context.MemberName ?? "(object)";
                    var members = (r.MemberNames?.Any() == true)
                        ? r.MemberNames.Select(m => $"{prefix}.{m}")
                        : new[] { prefix };

                    allErrors.Add(new ValidationResult(r.ErrorMessage, members));
                }
            }

            return allErrors.Count == 0 ? ValidationResult.Success : new CompositeValidationResult($"Validation failed for {context.MemberName ?? "(object)"}", allErrors);
        }

        // Helper that lets us keep nested results together (useful for logging/UIs)
        private sealed class CompositeValidationResult : ValidationResult {
            public IReadOnlyCollection<ValidationResult> Results { get; }
            public CompositeValidationResult(string message, IEnumerable<ValidationResult> results)
                : base(message) => Results = results.ToArray();
        }

    }

}