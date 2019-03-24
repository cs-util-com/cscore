#if NET_2_0 || NET_2_0_SUBSET

using System.Diagnostics;

namespace com.csutil {
    public static class StopWatchCompatExtensions {

        public static void Restart(this Stopwatch self) {
            self.Reset();
            self.Start();
        }

    }
}

#endif