using System;
using System.Runtime.InteropServices;

namespace com.csutil {

    public static class DisposableExtensions {

        #if !DEBUG
        [Obsolete("Will always return false for NON DEBUG builds", true)]
        #endif
        /// <summary> Could be used inside of the Dispose method to determine if Dispose was called as part of a thrown exception.
        /// Will not work on some platforms like Unity but can be useful for XUnit tests and logging </summary>
        public static bool DEBUG_ThrownExceptionDetectedInCurrentContext(this IDisposable _) { return DEBUG_ThrownExceptionDetectedInCurrentContext(); }

        public static bool DEBUG_ThrownExceptionDetectedInCurrentContext() {
        #if DEBUG
            try {
                return Marshal.GetExceptionCode() != 0;
            } catch (PlatformNotSupportedException) { }
        #endif
            return false;
        }

        public static bool IsActive(this IsDisposable self) { return self.IsDisposed == IsDisposable.State.Active; }

    }

}