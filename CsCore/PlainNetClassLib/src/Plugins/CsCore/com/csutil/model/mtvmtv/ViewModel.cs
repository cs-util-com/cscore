using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace com.csutil.model.mtvmtv {

    [Serializable]
    public class ViewModel {

        /// <summary> Will contain the type like "Object", "Integer", "Array", .. </summary>
        public string type;
        /// <summary> This will contain the concrete name of the model if type is an "Object" </summary>
        public string modelType;

        public List<string> order;
        public List<string> required;
        public Dictionary<string, ViewModel> properties;

        public string title;
        public string description;
        [JsonProperty("default")]
        public string defaultVal;

        public bool? readOnly;
        public bool? writeOnly;
        public bool? mandatory; // Not part of JSON schema and redundant with required list above
                                /// <summary> Regex pattern that has to be matched by the field value to be valid</summary>
        public string pattern;
        public string contentType;
        /// <summary> If the field is an object it has a view model itself </summary>

        public List<ViewModel> items;
        /// <summary> If true items is a set so it can only contain unique items </summary>
        public bool? uniqueItems;
        /// <summary> Controls whether it's valid to have additional items in the array </summary>
        public bool? additionalItems;

        /// <summary> Indicates that the field can only have descrete values </summary>
        [JsonProperty("enum")]
        public string[] contentEnum;

    }

    public enum ContentType { Alphanumeric, Name, Email, Password, Pin, Essay, Color, Date }

}