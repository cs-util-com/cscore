using System;

namespace com.csutil {

    /// <summary> 
    /// Add this to your properties to show them in the Unity inspector. 
    /// This attribute is only needed if DISABLE_CSUTIL_PROPERTY_MAGIC is set to true, otherwise
    /// all properties will be shown in the inspector automatically
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ShowInInspectorAttribute : Attribute {
    }

}
