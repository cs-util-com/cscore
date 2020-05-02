using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using com.csutil.logging;

namespace com.csutil {

    public static class Log {

        // The log system instance is not accessed via injection to avoid loops because 
        // the injection logic uses the logging logic itself
        public static ILog instance = new LogToConsole();

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        public static void d(string msg, params object[] args) {
            instance.LogDebug(msg, ArgsPlusStackFrameIfNeeded(args));
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        public static void w(string warning, params object[] args) {
            instance.LogWarning(warning, ArgsPlusStackTraceIfNeeded(args));
        }

        public static Exception e(string error, params object[] args) {
            return instance.LogError(error, ArgsPlusStackTraceIfNeeded(args));
        }

        private static bool IsTraceIncludedIn(object[] args) { return args.Get<StackFrame>() != null || args.Get<StackTrace>() != null; }

        public static Exception e(Exception e, params object[] args) {
            return instance.LogExeption(e, ArgsPlusStackTraceIfNeeded(args));
        }

        private static object[] ArgsPlusStackFrameIfNeeded(object[] args, int skipFrames = 2) {
            return IsTraceIncludedIn(args) ? args : new StackFrame(skipFrames, true).AddTo(args);
        }

        private static object[] ArgsPlusStackTraceIfNeeded(object[] args, int skipFrames = 2) {
            return IsTraceIncludedIn(args) ? args : new StackTrace(skipFrames, true).AddTo(args);
        }

        public static string CallingMethodStr(object[] args = null, int offset = 3, int count = 3) {
            var frame = args.Get<StackFrame>(); if (frame != null) { return frame.ToStringV2(); }
            var trace = args.Get<StackTrace>(); if (trace != null) { return trace.ToStringV2(count: count); }
            return new StackTrace(true).ToStringV2(offset, count);
        }

        private static T Get<T>(this object[] args) { return args != null ? (T)args.FirstOrDefault(x => x is T) : default(T); }

        public static StopwatchV2 MethodEntered([CallerMemberName] string methodName = null, params object[] args) {
            return instance.LogMethodEntered(methodName, args);
        }

        public static StopwatchV2 MethodEnteredWith(object arg1, object arg2 = null, object arg3 = null, object arg4 = null, [CallerMemberName] string n = null) {
            return MethodEntered(n, arg1, arg2, arg3, arg4);
        }

        public static void MethodDoneWith(Stopwatch timing, object arg1, object arg2 = null, object arg3 = null, int maxAllowedTimeInMs = -1,
                    [CallerMemberName] string sourceMemberName = null,
                    [CallerFilePath] string sourceFilePath = null,
                    [CallerLineNumber] int sourceLineNumber = 0) {
            var args = new object[3] { arg1, arg2, arg3 };
            instance.LogMethodDone(timing, args, maxAllowedTimeInMs, sourceMemberName, sourceFilePath, sourceLineNumber);
        }

        public static void MethodDone(Stopwatch timing, int maxAllowedTimeInMs = -1,
                    [CallerMemberName] string sourceMemberName = null,
                    [CallerFilePath] string sourceFilePath = null,
                    [CallerLineNumber] int sourceLineNumber = 0) {
            instance.LogMethodDone(timing, null, maxAllowedTimeInMs, sourceMemberName, sourceFilePath, sourceLineNumber);
        }

        public static string ToArgsStr(object[] args, Func<object, string> toString) {
            if (args.IsNullOrEmpty()) { return ""; }
            var s = args.ToStringV2(toString, "", "");
            if (s.IsNullOrEmpty()) { return ""; }
            return " : [[" + s + "]]";
        }

        public static void AddLoggerToLogInstances(ILog loggerToAdd) {
            if (Log.instance is LogToMultipleLoggers existingInstance) {
                existingInstance.loggers.Add(loggerToAdd);
            } else {
                var newInstance = new LogToMultipleLoggers();
                newInstance.loggers.Add(Log.instance);
                newInstance.loggers.Add(loggerToAdd);
                Log.instance = newInstance;
            }
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
                if (EnvironmentV2.isWebGL) { return "" + self; }
                var method = self.GetMethod(); // analyse stack trace for class name:
                var methodString = method.ReflectedType.Name + "." + method.Name;
                var paramsString = includeParams ? method.GetParameters().ToStringV2(x => "" + x, "", "") : "..";
                return methodString + "(" + paramsString + ")";
            }
            catch (Exception e) { Console.WriteLine("" + e); return ""; }
        }

        internal static object[] AddTo(this StackFrame self, object[] args) { return Add(args, self); }
        internal static object[] AddTo(this StackTrace self, object[] args) { return Add(args, self); }

        private static object[] Add(object[] args, Object obj) {
            var a = new object[1] { obj };
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