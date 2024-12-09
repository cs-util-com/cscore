using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace com.csutil {

    public static class AssertV3 {

        private const string LB = "\r\n";
        private static object syncLock = new object();

        public static bool throwExeptionIfAssertionFails = false;

        /// <summary> Allows an easy way to do print debugging for all successful assertions present in the 
        /// code to allow printing feedback that an Assert statement was successfully evaluated and did NOT fail. 
        /// When print debugging its not only important to see failing assertions but also that the code these 
        /// assertions are placed in is executed in general. Logging the assertions allows to use Assertions for 
        /// print debugging and at the same time motivates to write print debug lines that after the 
        /// debugging session can stay in the code </summary>
        public static Action<string, object[]> onAssertSuccess;

        private static void Assert(bool condition, Func<string> errorMsg, object[] args) {
            if (!condition) {
                args = new StackTrace(2, true).AddTo(args);
                Fail(errorMsg(), args);
            } else if (onAssertSuccess != null) {
                args = new StackTrace(2, true).AddTo(args);
                onAssertSuccess(errorMsg(), args); // If callback set inform it on success
            }
        }

        private static void Fail(string errorMsg, object[] args) {
            Debugger.Break();
            var e = Log.e(errorMsg, args);
            if (throwExeptionIfAssertionFails) { throw e; }
        }

        public static void Throws<T>(Action actionThatShouldThrowAnException) where T : Exception {
            try { actionThatShouldThrowAnException(); } catch (Exception e) {
                if (e is T) { return; } // its the expected exception
                throw; // its an unexpected exception, so rethrow it
            }
            throw new ThrowsException($"No exception of type {typeof(T)} was thrown!");
        }

        [Serializable]
        public class ThrowsException : Exception {
            public ThrowsException() : base() { }
            public ThrowsException(string message) : base(message) { }
            public ThrowsException(string message, Exception innerException) : base(message, innerException) { }
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsTrue(bool condition, Func<string> errorMsg, params object[] args) {
            Assert(condition, () => "Assert.IsTrue() FAILED: " + errorMsg(), args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsFalse(bool condition, Func<string> errorMsg, params object[] args) {
            Assert(!condition, () => "Assert.IsFalse() FAILED: " + errorMsg(), args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsEqualToPersisted(string id, object o, params object[] args) {
            args = new StackTrace(1, true).AddTo(args);
            var persisted = IoC.inject.GetOrAddSingleton<PersistedRegression>(o);
            persisted.AssertEqualToPersisted(id, o).ContinueWithSameContext((Task t) => {
                if (t.Exception != null) { Fail(t.Exception.Message, args); }
            });
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsNull(object o, string varName, params object[] args) {
            Assert(o == null, () => $"Assert.IsNull({varName}) FAILED, instead was: " + o, args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsNotNull(object o, string varName, params object[] args) {
            Assert(o != null, () => $"Assert.IsNotNull({varName}) FAILED", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreEqual(bool expected, bool actual, string varName = "", params object[] args) {
            Assert(expected == actual, () => $"Assert.AreEqual() FAILED: Actual {varName}= {actual} NOT equal to expected bool= {expected}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreEqual(long expected, long actual, string varName = "", params object[] args) {
            Assert(expected == actual, () => $"Assert.AreEqual() FAILED: Actual {varName}= {actual} NOT equal to expected number= {expected}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreEqual(double expected, double actual, string varName = "", params object[] args) {
            Assert(expected == actual, () => $"Assert.AreEqual() FAILED: Actual {varName}= {actual} NOT equal to expected number= {expected}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreEqual<T>(T expected, T actual, string varName = "", params object[] args) {
            if (expected is IEnumerable<T> expectedList && actual is IEnumerable<T> actualList && !(expected is string)) {
                if (!expected.Equals(actual)) {
                    string errorMsg = $"Assert.AreEqual() FAILED for {varName}: {CalcMultiLineUnequalText(expectedList, actualList)}";
                    Assert(false, () => errorMsg, args);
                }
            } else {
                Assert(Equals(expected, actual), () => $"Assert.AreEqual() FAILED: Actual {varName}= {actual} NOT equal to expected {typeof(T)}= {expected}", args);
            }
        }
        
        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsType<T>(object o, string varName = "", params object[] args) {
            Assert(o is T, () => $"Assert.IsType() FAILED: {varName} is not of type {typeof(T)}", args);
        }
        
        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsAssignableFrom<T>(object o, string varName = "", params object[] args) {
            Assert(typeof(T).IsAssignableFrom(o.GetType()), () => $"Assert.IsAssignableFrom() FAILED: {varName} is not assignable from {typeof(T)}", args);
        } 

        private static string CalcMultiLineUnequalText<T>(IEnumerable<T> expected, IEnumerable<T> actual, string _ = LB + "   ") {
            int diffPos = GetPosOfFirstDiff(expected, actual);
            int spacesCount = diffPos - 1;
            if (!(expected is string)) { spacesCount = spacesCount * 3 + 1; }
            string spaces = new string(' ', spacesCount);
            return $"{_}           {spaces}↓(pos {diffPos})" +
                $"{_} Expected: {expected.ToStringV2(x => "" + x)} " +
                $"{_} Actual:   {actual.ToStringV2(x => "" + x)}" +
                $"{_}           {spaces}↑(pos {diffPos})";
        }

        private static int GetPosOfFirstDiff<T>(IEnumerable<T> expected, IEnumerable<T> actual) {
            return expected.Zip(actual, (e1, e2) => e1.Equals(e2)).TakeWhile(b => b).Count() + 1;
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreNotEqual(Vector3 expected, Vector3 actual, string varName = "", params object[] args) {
            Assert(!Equals(expected, actual), () => $"Assert.AreNotEqual() FAILED: expected number {varName}= {expected} IS equal to actual {varName}= {actual}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreNotEqual(double expected, double actual, string varName = "", params object[] args) {
            Assert(!Equals(expected, actual), () => $"Assert.AreNotEqual() FAILED: expected number {varName}= {expected} IS equal to actual {varName}= {actual}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreNotEqual(float expected, float actual, string varName = "", params object[] args) {
            Assert(!Equals(expected, actual), () => $"Assert.AreNotEqual() FAILED: expected number {varName}= {expected} IS equal to actual {varName}= {actual}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreNotEqual(long expected, long actual, string varName = "", params object[] args) {
            Assert(!Equals(expected, actual), () => $"Assert.AreNotEqual() FAILED: expected number {varName}= {expected} IS equal to actual {varName}= {actual}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreNotEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string varName = "", params object[] args) {
            var isNotSameRef = !ReferenceEquals(expected, actual);
            Assert(isNotSameRef, () => $"Assert.AreNotEqual() FAILED: {varName} is same reference (expected {expected} == actual {actual} )", args);
            if (isNotSameRef) {
                Assert(!Equals(expected, actual), () => $"Assert.AreNotEqual() FAILED: expected  {varName}= {expected} IS equal to actual {varName}= {actual}", args);
            }
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreEqualJson(object a, object b, params object[] args) {
            if (ReferenceEquals(a, b)) { throw new ArgumentException("Both references pointed to the same object"); }
            var jsonDiff = MergeJson.GetDiffV2(a, b);
            Assert(MergeJson.HasNoDifferences(jsonDiff), () => "Difference found:\n" + jsonDiff?.ToPrettyString(), args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AreNotEqualLists<T>(IEnumerable<T> expected, IEnumerable<T> actual, string varName = "", params object[] args) {
            Assert(expected != actual, () => "Assert.AreNotEqual() FAILED: " + varName + " is same reference (expected == actual)", args);
            Assert(!expected.SequenceEqual(actual), () => $"Assert.AreNotEqual() FAILED: expected {varName}= {expected} IS equal to actual {varName}= {actual}", args);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void IsInRange(double lowerBound, double value, double upperBound, string varName, params object[] args) {
            if (upperBound < lowerBound) { throw Log.e($"Invalid bounds: (upperBound){upperBound} < {lowerBound}(lowerBound)"); }
            Assert(lowerBound <= value, () => $"Assert.IsInRange() FAILED: {varName}={value} is BELOW lower bound=" + lowerBound, args);
            Assert(value <= upperBound, () => $"Assert.IsInRange() FAILED: {varName}={value} is ABOVE upper bound=" + upperBound, args);
        }

        public static StopwatchV2 TrackTiming([CallerMemberName] string methodName = null) { return new StopwatchV2(methodName).StartV2(); }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AssertUnderXms(this Stopwatch self, int maxTimeInMs, params object[] args) {
            var ms = self.ElapsedMilliseconds;
            int p = (int)(ms * 100f / maxTimeInMs);
            Assert(IsUnderXms(self, maxTimeInMs), () => $"{GetMethodName(self)} took {p}% ({ms}ms) longer then allowed ({maxTimeInMs}ms)!", args);
        }

        private static string GetMethodName(Stopwatch s) {
            if (s is StopwatchV2 sV2) { return sV2.methodName; } else { return new StackFrame(2).GetMethodName(false); }
        }

        public static bool IsUnderXms(this Stopwatch self, int maxTimeInMs) { return self.ElapsedMilliseconds <= maxTimeInMs; }

        public static void ThrowExeptionIfAssertionFails(Action taskToExecute) {
            ThrowExeptionIfAssertionFails(true, taskToExecute);
        }

        public static void ThrowExeptionIfAssertionFails(bool shouldThrow, Action taskToExecute) {
            lock (syncLock) {
                var oldVal = throwExeptionIfAssertionFails;
                throwExeptionIfAssertionFails = shouldThrow;
                taskToExecute();
                throwExeptionIfAssertionFails = oldVal;
            }
        }

        /// <summary> Log out successful assetions, <see cref="AssertV3.onAssertSuccess"/> when this is useful </summary>
        public static void SetupPrintDebuggingSuccessfulAssertions() {
            AssertV3.onAssertSuccess = (msg, args) => {
                Log.d($"SUCCESSFUL ASSERT, did NOT throw: <<<{msg}>>>", Log.ArgsPlusStackFrameIfNeeded(args, skipFrames: 3));
            };
        }

    }
}