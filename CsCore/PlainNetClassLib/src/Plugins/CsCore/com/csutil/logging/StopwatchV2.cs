using System;
using System.Diagnostics;
using com.csutil.io;

namespace com.csutil {
    public class StopwatchV2 : Stopwatch {
        private long managedMemoryAtStart;
        private long managedMemoryAtStop;
        private long memoryAtStart;
        private long memoryAtStop;
        public long allocatedManagedMemBetweenStartAndStop { get { return managedMemoryAtStop - managedMemoryAtStart; } }
        public long allocatedMemBetweenStartAndStop { get { return memoryAtStop - memoryAtStart; } }

        public StopwatchV2 StartV2() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            managedMemoryAtStart = GC.GetTotalMemory(true);
            using (var p = Process.GetCurrentProcess()) { memoryAtStart = p.PrivateMemorySize64; }
            Start();
            return this;
        }

        public void StopV2() {
            Stop();
            managedMemoryAtStop = GC.GetTotalMemory(true);
            using (var p = Process.GetCurrentProcess()) { memoryAtStop = p.PrivateMemorySize64; }
        }

        public string GetAllocatedMemBetweenStartAndStop() {
            return "allocated managed mem: " + ByteSizeToString.ByteSizeToReadableString(allocatedManagedMemBetweenStartAndStop)
                + ", allocated mem: " + ByteSizeToString.ByteSizeToReadableString(allocatedMemBetweenStartAndStop);
        }

    }
}