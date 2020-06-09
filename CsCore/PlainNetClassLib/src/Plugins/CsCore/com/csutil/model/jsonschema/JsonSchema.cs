using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace com.csutil.model.jsonschema {

    /// <summary> A C# class that can hold a JSON schema, see 
    /// https://json-schema.org/understanding-json-schema/reference/index.html for specifics 
    /// of the structure of the schema. In summary its a recursive structure which describes each property 
    /// of a target model like the type and name of each property/field </summary>
    public class JsonSchema {

        /// <summary> Will contain the type like "object", "integer", "array", .. see also
        /// https://json-schema.org/understanding-json-schema/reference/type.html </summary>
        public string type;

        /// <summary> This will contain the concrete name of the model if type is an "Object" </summary>
        public string modelType; // Not part of official schema

        /// <summary> The list of required properties that cant be ommited, see also
        /// https://json-schema.org/understanding-json-schema/reference/object.html#required-properties </summary>
        public List<string> required;

        /// <summary> all properties/fields of the object are defined in the properties list, see also
        /// https://json-schema.org/understanding-json-schema/reference/object.html#properties </summary>
        public Dictionary<string, JsonSchema> properties;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/generic.html#annotations </summary>
        public string title;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/generic.html#annotations </summary>
        public string description;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/generic.html#annotations </summary>
        [JsonProperty("default")]
        public string defaultVal;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/numeric.html#range </summary>
        public float? minimum;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/numeric.html#range </summary>
        public float? maximum;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/string.html#length </summary>
        public int? minLength;
        /// <summary> https://json-schema.org/understanding-json-schema/reference/string.html#length </summary>
        public int? maxLength;

        /// <summary> Indicates that the element is read only and should never change, useful e.g. for IDs </summary>
        public bool? readOnly; // Not part of JSON schema

        /// <summary> Indicates that the element is write only, useful to mark fields that should never exported </summary>
        public bool? writeOnly; // Not part of JSON schema

        /// <summary> Redundant with required list of parent, so if true must also be contained in parent.required </summary>
        public bool? mandatory; // Not part of JSON schema and redundant with required list above

        /// <summary> Regex pattern that has to be matched by the field value to be valid, see also 
        /// https://json-schema.org/understanding-json-schema/reference/regular_expressions.html </summary>
        public string pattern;

        /// <summary> https://json-schema.org/understanding-json-schema/reference/string.html#format </summary>
        public string format;

        /// <summary> If the field is an object it has a view model itself, see also
        /// https://json-schema.org/understanding-json-schema/reference/array.html#items </summary>
        public List<JsonSchema> items;

        /// <summary> If true items is a set so it can only contain unique items, see also
        /// https://json-schema.org/understanding-json-schema/reference/array.html#uniqueness </summary>
        public bool? uniqueItems;

        /// <summary> Controls whether it's valid to have additional items in the array, see also
        /// https://json-schema.org/understanding-json-schema/reference/array.html#items and
        /// https://json-schema.org/understanding-json-schema/reference/array.html#tuple-validation </summary>
        public bool? additionalItems;

        /// <summary> Indicates that the field can only have descrete values, see also
        /// https://json-schema.org/understanding-json-schema/reference/generic.html#enumerated-values </summary>
        [JsonProperty("enum")]
        public string[] contentEnum;

        public static string ToTitle(string varName) { return RegexUtil.SplitCamelCaseString(varName); }

    }

    /// <summary> https://json-schema.org/understanding-json-schema/reference/string.html#built-in-formats </summary>
    public enum ContentFormat { alphanumeric, name, email, password, pin, essay, color, date, uri }

}