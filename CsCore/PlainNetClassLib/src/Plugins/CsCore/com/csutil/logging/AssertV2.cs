using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace com.csutil {
    public static class AssertV2 {

        [Conditional("DEBUG")]
        public static void IsTrue(bool condition, string message, params object[] args) {
            if (!condition) { Log.e(message, args); Debugger.Break(); }
        }

        [Conditional("DEBUG")]
        public static void IsFalse(bool condition, string message, params object[] args) {
            IsTrue(!condition, message, args);
        }

        public static Stopwatch TrackTiming() { return Stopwatch.StartNew(); }

        [Conditional("DEBUG")]
        public static void AssertUnderXms(this Stopwatch self, int maxTimeInMs) {
            float ms = self.ElapsedMilliseconds;
            var p = ms * 100f / maxTimeInMs;
            var errorText = GetCallingMethodName() + " took " + p + "% (" + ms + "ms) longer then allowed (" + maxTimeInMs + "ms)!";
            IsTrue(IsUnderXms(self, maxTimeInMs), errorText);
        }

        public static bool IsUnderXms(this Stopwatch self, int maxTimeInMs) { return self.ElapsedMilliseconds <= maxTimeInMs; }

        private static string GetCallingMethodName(int i = 2) { return new StackTrace().GetFrame(i).GetMethodName(); }

        /// <summary> Will return a formated string in the form of ClassName.MethodName </summary>
        public static string GetMethodName(this StackFrame self) {
            var method = self.GetMethod(); // analyse stack trace for class name:
            var paramsString = method.GetParameters().ToStringV2(x => "" + x, "", "");
            return method.ReflectedType.Name + "." + method.Name + "(" + paramsString + ")";
        }

        [Conditional("DEBUG")]
        public static void IsNull(object o, string varName, params object[] args) {
            string errorMsg = "Assert.IsNull(" + varName + ") failed";
            IsTrue(o == null, errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void IsNotNull(object o, string varName, params object[] args) {
            string errorMsg = "Assert.IsNotNull(" + varName + ") failed";
            IsTrue(o != null, errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void AreEqual<T>(IEquatable<T> expected, IEquatable<T> actual, string varName = "", params object[] args) {
            var errorMsg = "Assert.AreEqual failed: expected " + varName + "=" + expected + " NOT equal actual " + varName + "=" + actual;
            IsTrue(expected.Equals(actual), errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void AreNotEqual<T>(IEquatable<T> expected, IEquatable<T> actual, string varName = "", params object[] args) {
            var errorMsg = "Assert.AreNotEqual failed: expected " + varName + "=" + expected + " IS equal to actual " + varName + "=" + actual;
            IsTrue(!expected.Equals(actual), errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void AreNotEqualLists<T>(IEnumerable<T> expected, IEnumerable<T> actual, string varName = "", params object[] args) {
            string msg1 = "Assert.AreNotEqual failed: " + varName + " same reference (expected == actual)";
            IsTrue(expected != actual, msg1, args);
            string msg2 = "Assert.AreNotEqual failed: expected " + varName + "=" + expected + " IS equal to actual " + varName + "=" + actual;
            IsTrue(!expected.SequenceEqual(actual), msg2, args);
        }

    }
}