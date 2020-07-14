using System.Reflection;
using UnityEngine.Events;

namespace com.csutil {

    public static class UnityEventExtensions {

        /// <summary> Adds a child GameObject to the calling new parent GameObject </summary>
        public static bool IsNullOrEmpty(this UnityEventBase self) {
            if (self == null) { return true; }
            return self.GetAllEventCount() == 0;
        }

        // https://forum.unity.com/threads/get-number-of-runtime-listeners.292537/#post-5623144
        public static int GetAllEventCount(this UnityEventBase unityEvent) {
            var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var invokeCallList = field.GetValue(unityEvent);
            var property = invokeCallList.GetType().GetProperty("Count");
            return (int)property.GetValue(invokeCallList);
        }

    }

}