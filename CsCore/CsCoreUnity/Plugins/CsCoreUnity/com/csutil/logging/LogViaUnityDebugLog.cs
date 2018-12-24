using com.csutil.logging;
using System;
using System.Linq;
using UnityEngine;

namespace com.csutil {

    public class LogViaUnityDebugLog : ILog {

        private const string LB = "\r\n";

        public void LogDebug(string msg, params object[] args) {
            Debug.Log(msg + Log.ToArgsStr(args, toString) + LB
                + " at " + Log.CallingMethodStr(args) + LB, getGoFrom(args));
        }

        public void LogWarning(string warning, params object[] args) {
            Debug.LogWarning("> WARNING:" + warning + Log.ToArgsStr(args, toString) + LB
                + " at " + Log.CallingMethodStr(args) + LB, getGoFrom(args));
        }

        public Exception LogError(string error, params object[] args) {
            Debug.LogError(">>> ERROR " + error + Log.ToArgsStr(args, toString) + LB
                + " at " + Log.CallingMethodStr(args) + LB, getGoFrom(args));
            return new Exception(error);
        }

        public Exception LogExeption(Exception e, params object[] args) {
            Debug.LogException(e, getGoFrom(args));
            return e;
        }

        private static GameObject getGoFrom(object[] args) {
            return args.Filter(x => x is GameObject).FirstOrDefault() as GameObject;
        }

        private string toString(object arg) {
            if (arg is System.Diagnostics.StackFrame) { return null; }
            if (arg is GameObject) { return null; }
            return "" + arg;
        }
    }

}