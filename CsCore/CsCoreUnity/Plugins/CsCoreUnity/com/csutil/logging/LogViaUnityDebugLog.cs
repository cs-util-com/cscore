using System;
using System.Linq;
using UnityEngine;

namespace com.csutil.logging {

    public class LogViaUnityDebugLog : LogDefaultImpl {

        protected override void PrintDebugMessage(string debugLogMsg, object[] args) {
            Debug.Log(debugLogMsg, getGoFrom(args));
        }

        protected override void PrintWarningMessage(string warningMsg, object[] args) {
            Debug.LogWarning(warningMsg, getGoFrom(args));
        }

        protected override void PrintErrorMessage(string errorMsg, object[] args) {
            Debug.LogError(errorMsg, getGoFrom(args));
        }

        protected override void PrintException(Exception e, object[] args) {
            Debug.LogException(e, getGoFrom(args));
        }

        protected override string ToString(object arg) {
            if (arg is System.Diagnostics.StackFrame) { return null; }
            if (arg is GameObject) { return null; }
            return "" + arg;
        }

        private static GameObject getGoFrom(object[] args) {
            return args.Filter(x => x is GameObject).FirstOrDefault() as GameObject;
        }

    }

}