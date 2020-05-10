using System;
using System.Linq;

namespace com.csutil.model.mtvmtv {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute {
        public string description;
        public DescriptionAttribute(string description) { this.description = description; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RegexAttribute : Attribute {

        public string[] regex;
        public RegexAttribute(params string[] regex) { this.regex = regex; }

        public RegexAttribute(int minChars, params string[] regex) { SetRegex(regex, "^.{" + minChars + ",}$"); }
        public RegexAttribute(int minChars, int maxChars, params string[] regex) { SetRegex(regex, "^.{" + minChars + "," + maxChars + "}$"); }

        private void SetRegex(string[] regex, string s) {
            var minMaxRegex = new string[1] { s };
            this.regex = minMaxRegex.Union(regex).ToArray();
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ContentAttribute : DescriptionAttribute {
        public ContentType type;
        public ContentAttribute(ContentType type, string description) : base(description) { this.type = type; }
    }

}