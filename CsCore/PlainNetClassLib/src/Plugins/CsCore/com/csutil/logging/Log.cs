using System;
using System.Diagnostics;
using System.Linq;
using com.csutil.logging;

namespace com.csutil {

    public static class Log {

        // The log system instance is not accessed via injection to avoid loops because 
        // the injection logic uses the logging logic itself
        public static ILog instance = new LogViaConsole();

        [Conditional("DEBUG")]
        public static void d(string msg, params object[] args) {
            instance.LogDebug(msg, args);
        }

        [Conditional("DEBUG")]
        public static void w(string warning, params object[] args) {
            instance.LogWarning(warning, args);
        }

        public static Exception e(string error, params object[] args) {
            return instance.LogError(error, args);
        }

        public static Exception e(Exception e, params object[] args) {
            return instance.LogExeption(e, args);
        }

        public static string CallingMethodStr(object[] args = null, int offset = 3, int count = 3) {
            var frame = args.Get<StackFrame>(); if (frame != null) { return frame.ToStringV2(); }
            var trace = args.Get<StackTrace>(); if (trace != null) { return trace.ToStringV2(); }
            return new StackTrace(true).ToStringV2(offset, count);
        }

        private static T Get<T>(this object[] args) { return args != null ? (T)args.FirstOrDefault(x => x is T) : default(T); }

        public static Stopwatch MethodEntered(params object[] args) {
#if DEBUG
            var t = new StackFrame(1, true);
            Log.d(" --> " + t.GetMethodName(false), t.AddTo(args));
#endif
            return AssertV2.TrackTiming();
        }

        [Conditional("DEBUG")]
        public static void MethodDone(Stopwatch timing, int maxAllowedTimeInMs = -1) {
            timing.Stop();
            var t = new StackFrame(1, true);
            Log.d("    <-- " + t.GetMethodName(false) + " finished after " + timing.ElapsedMilliseconds + "ms", t.AddTo(null));
            if (maxAllowedTimeInMs > 0) { timing.AssertUnderXms(maxAllowedTimeInMs); }
        }

        public static string ToArgsStr(object[] args, Func<object, string> toString) {
            if (args.IsNullOrEmpty()) { return ""; }
            var s = args.ToStringV2(toString, "", "");
            if (s.IsNullOrEmpty()) { return ""; }
            return " : [[" + s + "]]";
        }

    }

    public static class LogExtensions {

        private const string LB = "\r\n";

        public static Exception PrintStackTrace(this Exception self, params object[] args) {
            return Log.e(self, args);
        }

        /// <summary> Will return a formated string in the form of ClassName.MethodName </summary>
        public static string GetMethodName(this StackFrame self, bool includeParams = true) {
            try {
                var method = self.GetMethod(); // analyse stack trace for class name:
                var methodString = method.ReflectedType.Name + "." + method.Name;
                var paramsString = includeParams ? method.GetParameters().ToStringV2(x => "" + x, "", "") : "..";
                return methodString + "(" + paramsString + ")";
            } catch (Exception e) { Console.WriteLine("" + e); return ""; }
        }

        internal static object[] AddTo(this StackFrame stackFrame, object[] args) {
            var a = new object[1] { stackFrame };
            if (args == null) { return a; } else { return args.Concat(a).ToArray(); }
        }

        public static string ToStringV2(this StackTrace self, int offset = 0, int count = -1) {
            if (count <= 0) { count = self.FrameCount; }
            var result = "";
            for (int i = offset; i < offset + count; i++) {
                if (i >= self.FrameCount) { break; }
                result += self.GetFrame(i).ToStringV2() + LB + "     ";
            }
            return result;
        }

        public static string ToStringV2(this StackFrame f) {
            return f.GetMethodName() + " " + f.GetFileName() + ":line " + f.GetFileLineNumber();
        }

    }

}