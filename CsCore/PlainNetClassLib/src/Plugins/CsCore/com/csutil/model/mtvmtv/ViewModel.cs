using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace com.csutil.model.mtvmtv {

    [Serializable]
    public class ViewModel {

        public string title;
        /// <summary> Will contain the type like "Object", "Integer", "Array", .. </summary>
        public string type;
        /// <summary> This will contain the concrete name of the model if type is an "Object" </summary>
        public string modelType;
        public List<string> order;
        public Dictionary<string, Field> properties;

        [Serializable]
        public class Field {

            public string type;
            public string title;
            public string description;

            public bool? readOnly;
            public bool? writeOnly;
            public bool? mandatory;
            public string[] regex;
            public string contentType;
            /// <summary> If the field is an object it has a view model itself </summary>
            public ViewModel objVm;

            public List<ViewModel> items;
            /// <summary> If true items is a set so it can only contain unique items </summary>
            public bool? uniqueItems;

            /// <summary> Indicates that the field can only have descrete values </summary>
            [JsonProperty("enum")]
            public string[] contentEnum;

        }

    }

    public enum ContentType { Alphanumeric, Name, Email, Password, Pin, Essay }

}