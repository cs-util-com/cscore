using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace com.csutil.logging {

    public class LogToUnityDebugLog : LogDefaultImpl {

        protected override void PrintDebugMessage(string debugLogMsg, object[] args) {
            Debug.Log(debugLogMsg, GetGoFrom(args));
        }

        protected override void PrintWarningMessage(string warningMsg, object[] args) {
            Debug.LogWarning(warningMsg, GetGoFrom(args));
        }

        protected override void PrintErrorMessage(string errorMsg, object[] args) {
            Debug.LogError(errorMsg, GetGoFrom(args));
        }

        protected override void PrintException(Exception e, object[] args) {
            Debug.LogException(e, GetGoFrom(args));
        }

        protected override string ArgToString(object arg) {
            if (arg is GameObject) { return null; }
            return base.ArgToString(arg);
        }

        private static GameObject GetGoFrom(object[] args) {
            return args.Filter(x => x is GameObject).FirstOrDefault() as GameObject;
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