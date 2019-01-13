using System;

namespace com.csutil {

    /// <summary> Annotate any MonoBehaviour property to show it in the inspector </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ShowInInspectorAttribute : Attribute {

    }

}

