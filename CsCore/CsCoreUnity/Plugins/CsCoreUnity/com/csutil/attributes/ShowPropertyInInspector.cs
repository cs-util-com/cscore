using System;

namespace com.csutil {

    /// <summary> 
    /// Add this to your properties to show them in the Unity inspector. 
    /// This attribute is NOT needed if ENABLE_CSUTIL_PROPERTY_MAGIC is set to true,
    /// because then all properties will be shown in the inspector automatically
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ShowPropertyInInspector : Attribute {
    }

}
