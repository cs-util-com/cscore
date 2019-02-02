using System;
using System.Linq;
using UnityEngine;

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

    }

}