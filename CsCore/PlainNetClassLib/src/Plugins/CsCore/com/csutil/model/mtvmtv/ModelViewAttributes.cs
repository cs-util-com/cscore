using System;

namespace com.csutil.model.mtvmtv {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute {

        public string description;
        public DescriptionAttribute(string description) { this.description = description; }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RegexAttribute : Attribute {

        public string regex;
        public RegexAttribute(string regex) { this.regex = regex; }

    }

}