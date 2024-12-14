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
        public Action<Stopwatch> onDispose;
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

        public StopwatchV2(Action<Stopwatch> onDispose, [CallerMemberName] string methodName = null) {
            this.methodName = methodName;
            this.onDispose = onDispose;
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
            return new StopwatchV2(onDispose: t => Log.MethodDone(t), methodName).StartV2();
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
            this.ThrowErrorIfDisposed();
            var diff = ElapsedMilliseconds - lastLogStep;
            lastLogStep = ElapsedMilliseconds;
            if (diff > skipIfBelowXMs) {
                Log.d($"{prefix}{stepName} finished after {diff} ms");
            }
        }

        private int latestMaxTimeInMs;
        private Func<string> latestStepName;
        private object[] latestArgs;
        private long startTime;

        public void CompleteLatestStep() {
            if (latestStepName != null) {
                var ms = ElapsedMilliseconds - startTime;
                if (ms > latestMaxTimeInMs) {
                    int p = (int)(ms * 100f / latestMaxTimeInMs);
                    Log.e($"        +> {latestStepName()} took {p}% ({ms}ms) longer then allowed ({latestMaxTimeInMs}ms) in {methodName}!", latestArgs);
                }
                latestStepName = null;
            }
        }

        public Action<Func<string>, long, int, object[]> OnStepStart { get; set; }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public void AssertNextStepUnderXms(int maxTimeInMs, Func<string> stepName, params object[] args) {
            this.ThrowErrorIfDisposed();
            // First complete the previous step:
            CompleteLatestStep();
            // Then store the values for the next/new step:
            latestMaxTimeInMs = maxTimeInMs;
            latestStepName = stepName;
            latestArgs = args;
            startTime = ElapsedMilliseconds;
            OnStepStart?.Invoke(latestStepName, startTime, latestMaxTimeInMs, latestArgs);
        }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public void Dispose() {
            if (IsDisposed != DisposeState.Active) { return; }
            CompleteLatestStep();
            IsDisposed = DisposeState.DisposingStarted;
            onDispose?.Invoke(this);
            IsDisposed = DisposeState.Disposed;
        }

    }

}