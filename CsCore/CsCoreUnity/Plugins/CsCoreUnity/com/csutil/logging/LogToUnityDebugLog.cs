using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

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

    }

}