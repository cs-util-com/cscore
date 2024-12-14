using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace com.csutil {

    public class StopwatchV2 : Stopwatch, IDisposableV2 {

        private long managedMemoryAtStart;
        private long managedMemoryAtStop;
        private long memoryAtStart;
        private long memoryAtStop;
        public string methodName;
        public Action onDispose;
        private long lastLogStep = 0;

        public bool doMemoryLogging = false;
        public bool forceFullMemoryCollection = ShouldCaptureFullMemory();

        private static bool ShouldCaptureFullMemory() {
            #if UNITY_EDITOR
            {
                // In recent versions of the Unity editor memory collection causes massive main thread
                // freezing for at least 1 second per usage, so it cant be used anymore:
                return false;
            }
            #endif
            return true; // In all other environments logging memory usage is fine
        }

        public StopwatchV2([CallerMemberName] string methodName = null, Action onDispose = null) {
            this.methodName = methodName;
            this.onDispose = onDispose;
            if (this.onDispose == null) { this.onDispose = () => Log.MethodDone(this); }
        }

        public long allocatedManagedMemBetweenStartAndStop { get { return managedMemoryAtStop - managedMemoryAtStart; } }
        public long allocatedMemBetweenStartAndStop { get { return memoryAtStop - memoryAtStart; } }

        public StopwatchV2 StartV2() {
            CaptureMemoryAtStart();
            Start();
            return this;
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private void CaptureMemoryAtStart() {
            if (!doMemoryLogging) { return; }
            if (forceFullMemoryCollection) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            managedMemoryAtStart = GC.GetTotalMemory(forceFullMemoryCollection);
            memoryAtStart = GetCurrentProcessPrivateMemorySize64();
        }

        public static StopwatchV2 StartNewV2([CallerMemberName] string methodName = null) {
            return new StopwatchV2(methodName).StartV2();
        }

        public void StopV2() {
            Stop();
            CaptureMemoryAtStop();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private void CaptureMemoryAtStop() {
            if (!doMemoryLogging) { return; }
            managedMemoryAtStop = GC.GetTotalMemory(forceFullMemoryCollection);
            memoryAtStop = GetCurrentProcessPrivateMemorySize64();
        }

        private long GetCurrentProcessPrivateMemorySize64() {

            // WebGL does not support PrivateMemorySize64:
            if (EnvironmentV2.isWebGL) { return 0; }

            // In latest Unity versions PrivateMemorySize64 seems to not work anymore for Android, so disabled:
            if (EnvironmentV2.isAndroid && !EnvironmentV2.isUnityEditor) { return 0; }

            try {
                using (var p = Process.GetCurrentProcess()) { return p.PrivateMemorySize64; }
            } catch (Exception e) {
                Log.e("GetCurrentProcessPrivateMemorySize64 failed: " + e, e);
                return 0;
            }

        }

        public string GetAllocatedMemBetweenStartAndStop(bool returnEmtpyStringIfZero = false) {
            if (returnEmtpyStringIfZero && allocatedManagedMemBetweenStartAndStop == 0 && allocatedMemBetweenStartAndStop == 0) {
                return "";
            }
            return "allocated managed mem: " + ByteSizeToString.ByteSizeToReadableString(allocatedManagedMemBetweenStartAndStop)
                + ", allocated mem: " + ByteSizeToString.ByteSizeToReadableString(allocatedMemBetweenStartAndStop);
        }

        public override string ToString() {
            return $"StopWatch '{methodName}' ({ElapsedMilliseconds} ms)";
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        public void LogStep(string stepName, string prefix = "        +> ", int skipIfBelowXMs = 10) {
            var diff = ElapsedMilliseconds - lastLogStep;
            lastLogStep = ElapsedMilliseconds;
            if (diff > skipIfBelowXMs) {
                Log.d($"{prefix}{stepName} finished after {diff} ms");
            }
        }

        //[Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public void AssertStepUnderXms(int maxTimeInMs, Func<string> stepName, params object[] args) {
            var ms = ElapsedMilliseconds - lastLogStep;
            lastLogStep = ElapsedMilliseconds;
            if (ms > maxTimeInMs) {
                int p = (int)(ms * 100f / maxTimeInMs);
                Log.e($"        +> {stepName()} took {p}% ({ms}ms) longer then allowed ({maxTimeInMs}ms) in {methodName}!", args);
            }
        }
        
        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public void Dispose() {
            if (IsDisposed != DisposeState.Active) { return; }
            IsDisposed = DisposeState.DisposingStarted;
            onDispose?.Invoke();
            IsDisposed = DisposeState.Disposed;
        }

    }

}