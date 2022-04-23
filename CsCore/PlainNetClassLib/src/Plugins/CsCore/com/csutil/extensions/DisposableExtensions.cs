using System;
using System.Runtime.InteropServices;

namespace com.csutil {

    public static class DisposableExtensions {

        #if !DEBUG
        [Obsolete("Will always return false for NON DEBUG builds", true)]
        #endif
        /// <summary> Could be used inside of the Dispose method to determine if Dispose was called as part of a thrown exception </summary>
        public static bool DEBUG_ThrownExceptionDetectedInCurrentContext(this IDisposable _) { return DEBUG_ThrownExceptionDetectedInCurrentContext(); }

        public static bool DEBUG_ThrownExceptionDetectedInCurrentContext() {
        #if DEBUG
            try {
                return Marshal.GetExceptionCode() != 0;
            } catch (PlatformNotSupportedException e) {
                Log.w("DEBUG_ThrownExceptionDetectedInCurrentContext will always return false on this platform", e);
            }
        #endif
            return false;
        }

    }

}