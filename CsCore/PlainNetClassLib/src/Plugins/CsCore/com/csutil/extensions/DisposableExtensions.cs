using System;
using System.Collections.Generic;
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

        /// <summary> If the object is disposed (or currently in the process of being disposed) this will return false </summary>
        public static bool IsAlive(this IDisposableV2 self) { return self.IsDisposed == DisposeState.Active; }

        /// <summary> Disposes the object if needed (and returns true) or if already disposed returns false </summary>
        public static bool DisposeV2(this IDisposableV2 self) {
            if (self.IsDisposed == DisposeState.Active) {
                self.Dispose();
                return true;
            } else {
                return false;
            }
        }

        public static IDisposableV2 ToDisposableV2(this IDisposable self) {
            if (self is IDisposableV2 d2) { return d2; }
            return new IDisposableCollection(new List<IDisposable>() { self });
        }

        public static void ThrowErrorIfDisposed(this IDisposableV2 self) {
            if (self != null && self.IsDisposed != DisposeState.Active) {
                throw new ObjectDisposedException($"{self.GetType()} {self} is already " + self.IsDisposed);
            }
        }

    }

}