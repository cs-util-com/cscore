using System;
using System.Collections.Generic;

namespace com.csutil.model.mtvmtv {

    [Serializable]
    public class ViewModel {

        public string title;
        public string type;
        public List<string> order;
        public Dictionary<string, Field> properties;

        [Serializable]
        public class Field {

            //public Text text;
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
            public ChildList items;

            /// <summary> Indicates that the field can only have descrete values </summary>
            public string[] contentEnum;

            //[Serializable]
            //public class Text {
            //    public string name;
            //    public string descr;
            //}

            [Serializable]
            public class ChildList {
                public string type;
                public List<ViewModel> entries;
            }

        }

    }

    public enum ContentType { Alphanumeric, Name, Email, Password, Pin, Essay }

}