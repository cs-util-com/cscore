using System;

namespace Xunit {

    /// <summary>
    /// Mock for the Xunit Trait attribute, which is used to categorize tests.
    /// Annotation looks like this:
    /// [Trait("Category","SkipInBuildPipeline")]
    /// </summary>
    public class Trait : Attribute {

        public string Category { get; }
        public string Value { get; }

        public Trait(string category, string value) {
            Category = category;
            Value = value;
        }

    }

}