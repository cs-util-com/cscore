using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace com.csutil {
    public static class AssertV2 {

        private static void Assert(bool condition, string errorMsg, object[] args) {
            if (!condition) { Log.e(errorMsg, args); Debugger.Break(); }
        }

        [Conditional("DEBUG")]
        public static void IsTrue(bool condition, string errorMsg, params object[] args) {
            Assert(condition, "Assert.IsTrue() FAILED in " + GetCallingMethodName() + ": " + errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void IsFalse(bool condition, string errorMsg, params object[] args) {
            Assert(!condition, "Assert.IsFalse() FAILED in " + GetCallingMethodName() + ": " + errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void IsNull(object o, string varName, params object[] args) {
            string errorMsg = "Assert.IsNull(" + varName + ") FAILED in " + GetCallingMethodName();
            Assert(o == null, errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void IsNotNull(object o, string varName, params object[] args) {
            string errorMsg = "Assert.IsNotNull(" + varName + ") FAILED in " + GetCallingMethodName();
            Assert(o != null, errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void AreEqual<T>(IEquatable<T> expected, IEquatable<T> actual, string varName = "", params object[] args) {
            var errorMsg = "Assert.AreEqual() FAILED in " + GetCallingMethodName() + ": expected " + varName + "=" + expected + " NOT equal actual " + varName + "=" + actual;
            Assert(expected.Equals(actual), errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void AreNotEqual<T>(IEquatable<T> expected, IEquatable<T> actual, string varName = "", params object[] args) {
            var errorMsg = "Assert.AreNotEqual() FAILED in " + GetCallingMethodName() + ": expected " + varName + "=" + expected + " IS equal to actual " + varName + "=" + actual;
            Assert(!expected.Equals(actual), errorMsg, args);
        }

        [Conditional("DEBUG")]
        public static void AreNotEqualLists<T>(IEnumerable<T> expected, IEnumerable<T> actual, string varName = "", params object[] args) {
            string msg1 = "Assert.AreNotEqual() FAILED in " + GetCallingMethodName() + ": " + varName + " same reference (expected == actual)";
            Assert(expected != actual, msg1, args);
            string msg2 = "Assert.AreNotEqual() FAILED in " + GetCallingMethodName() + ": expected " + varName + "=" + expected + " IS equal to actual " + varName + "=" + actual;
            Assert(!expected.SequenceEqual(actual), msg2, args);
        }

        public static Stopwatch TrackTiming() { return Stopwatch.StartNew(); }

        [Conditional("DEBUG")]
        public static void AssertUnderXms(this Stopwatch self, int maxTimeInMs, params object[] args) {
            var ms = self.ElapsedMilliseconds;
            int p = (int)(ms * 100f / maxTimeInMs);
            var errorText = GetCallingMethodName() + " took " + p + "% (" + ms + "ms) longer then allowed (" + maxTimeInMs + "ms)!";
            Assert(IsUnderXms(self, maxTimeInMs), errorText, args);
        }

        public static bool IsUnderXms(this Stopwatch self, int maxTimeInMs) { return self.ElapsedMilliseconds <= maxTimeInMs; }

        private static string GetCallingMethodName(int i = 2) { return new StackTrace().GetFrame(i).GetMethodName(); }

        /// <summary> Will return a formated string in the form of ClassName.MethodName </summary>
        public static string GetMethodName(this StackFrame self) {
            var method = self.GetMethod(); // analyse stack trace for class name:
            var paramsString = method.GetParameters().ToStringV2(x => "" + x, "", "");
            return method.ReflectedType.Name + "." + method.Name + "(" + paramsString + ")";
        }

    }
}