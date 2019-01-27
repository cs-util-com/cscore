using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace com.csutil {
    public static class StopWatchCompatExtensions {

        public static void Restart(this Stopwatch self) {
            self.Reset();
            self.Start();
        }

    }
}
