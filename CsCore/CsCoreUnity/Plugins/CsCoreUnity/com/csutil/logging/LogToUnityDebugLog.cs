using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace com.csutil.logging {

    public class LogToUnityDebugLog : LogDefaultImpl {

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            Debug.Log(EditModePrefix() + debugLogMsg, GetUnityObjFrom(args));
        }

        protected override void PrintInfoMessage(string infoMsg, params object[] args) {
            Debug.Log(EditModePrefix() + infoMsg, GetUnityObjFrom(args));
        }

        protected override void PrintWarningMessage(string warningMsg, params object[] args) {
            Debug.LogWarning(EditModePrefix() + warningMsg, GetUnityObjFrom(args));
        }

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            Debug.LogError(EditModePrefix() + errorMsg, GetUnityObjFrom(args));
        }

        protected override void PrintException(Exception e, params object[] args) {
            Debug.LogException(e, GetUnityObjFrom(args));
        }

        private string EditModePrefix() {
#if UNITY_EDITOR
            if (!ApplicationV2.isPlaying) { return "[ EDIT MODE ]  "; }
#endif
            if (MainThread.isMainThread) { return "[ MAIN THREAD ] "; }
            return "";
        }

        protected override string ArgToString(object arg) {
            if (arg is GameObject) { return null; }
            return base.ArgToString(arg);
        }

        private static UnityEngine.Object GetUnityObjFrom(object[] args) {
            return args.Filter(x => x is UnityEngine.Object).FirstOrDefault() as UnityEngine.Object;
        }

        public override StopwatchV2 LogMethodEntered(string methodName, object[] args) {
            Profiler.BeginSample(methodName, args.FirstOrDefault(x => x is UnityEngine.Object) as UnityEngine.Object);
            return base.LogMethodEntered(methodName, args);
        }

        public override void LogMethodDone(System.Diagnostics.Stopwatch timing, object[] args, int maxAllowedTimeInMs,
                                                string sourceMemberName, string sourceFilePath, int sourceLineNumber) {
            Profiler.EndSample();
            base.LogMethodDone(timing, args, maxAllowedTimeInMs, sourceMemberName, sourceFilePath, sourceLineNumber);
        }

        private int backgroundThreadCounter = 0;

        public override StopwatchV2 BeginThreadProfiling() {
            Profiler.BeginThreadProfiling("Background Threads", "Task Thread " + ++backgroundThreadCounter);
            return new StopwatchV2(onDispose: self => {
                Profiler.EndThreadProfiling();
            });
        }

        public override StopwatchV2 TrackTiming(string methodName, Action<Stopwatch> onDispose) {
            var t = base.TrackTiming(methodName, onDispose);
            CustomSampler stepSampler = CustomSampler.Create(methodName);
            stepSampler.Begin();
            t.OnStepStart = (stepName, startTime, maxTime, args) => {
                if (stepSampler != null) {
                    stepSampler.End();
                }
                var samplerName = stepName() + " t-" + startTime;
                stepSampler = CustomSampler.Create(samplerName);
                stepSampler.Begin();
            };
            t.onDispose = self => {
                if (stepSampler != null) {
                    stepSampler.End();
                }
                onDispose?.Invoke(self);
            };
            return t;
        }

    }

}