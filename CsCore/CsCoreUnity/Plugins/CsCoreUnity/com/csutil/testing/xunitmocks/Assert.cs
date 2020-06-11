using com.csutil;
using com.csutil.math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xunit {

    public class AssertException : Exception {
        public AssertException() { }
        public AssertException(string message) : base(message) { }
        public AssertException(string message, Exception innerException) : base(message, innerException) {
        }
    }

    public static class Assert {

        public static void True(bool b, string msg = null) {
            if (!b) { throw (msg != null) ? new AssertException(msg) : new AssertException(); }
        }

        public static void False(bool b, string msg = null) { True(!b, msg); }

        public static void Null<T>(T obj) { True(null == obj, "Was NOT null: " + obj); }
        public static void NotNull<T>(T obj) { True(null != obj, "Was null: " + typeof(T)); }

        public static void Equal(object exp, object actual) {
            True(Eq(exp, actual), "NOT Equal: Expected\n " + exp + " \n but was \n " + actual);
        }

        public static void Equal<T>(IEnumerable<T> exp, IEnumerable<T> actual) {
            True(exp.SequenceEqual(actual), "IEnumerables NOT Equal: Expected\n "
                + AsString(exp) + " \n but was \n " + AsString(actual));
        }

        private static string AsString<T>(IEnumerable<T> e) { return e.ToStringV2(x => "" + x); }

        private static bool Eq(object objA, object objB) {
            if (ReferenceEquals(objA, objB)) { return true; }
            if (Numbers.HasNumberType(objA) && Numbers.HasNumberType(objB)) { return Convert.ToDouble(objA) == Convert.ToDouble(objB); }
            if (objA is IComparable c1 && objB is IComparable c2) { return c1.CompareTo(c2) == 0; }
            return objA == objB || Equals(objA, objB);
        }

        public static void NotEqual(object objA, object objB) {
            True(!Eq(objA, objB), "EQUAL: \n " + objA + " \n and \n " + objB);
        }

        public static void IsType<T>(object obj) where T : class {
            True(obj is T, "Not Type " + typeof(T) + ": " + obj);
        }

        public static void Throws<T>(Action a) where T : Exception {
            var notThrown = false;
            try { a(); notThrown = true; }
            catch (T) { }
            if (notThrown) { throw new AssertException("Did not throw " + typeof(T)); }
        }

        public static async Task ThrowsAsync<T>(Func<Task> a) where T : Exception {
            var notThrown = false;
            try { await a(); notThrown = true; }
            catch (T) { }
            if (notThrown) { throw new AssertException("Did not throw " + typeof(T)); }
        }

        public static void Same<T>(T expected, T actual) {
            True(ReferenceEquals(expected, actual));
        }

        public static void NotSame<T>(T objA, T objB) {
            True(!ReferenceEquals(objA, objB));
        }

        public static void InRange<T>(T actual, T min, T max) where T : IComparable {
            True(min.CompareTo(actual) <= 0 && actual.CompareTo(max) <= 0, "actual=" + actual + " not in range [" + min + " .. " + max + "]");
        }

        public static void Single<T>(IEnumerable<T> e) { Equal(1, e.Count()); }

        public static void Empty<T>(IEnumerable<T> e) { Equal(0, e.Count()); }
        public static void NotEmpty<T>(IEnumerable<T> e) { NotEqual(0, e.Count()); }

        public static void Contains(object obj, IEnumerable e) {
            foreach (var i in e) { if (Eq(obj, i)) { return; } }
            throw new AssertException("Not found in " + e + ":" + obj);
        }

        public static void Contains(string subString, string fullString) {
            if (!fullString.Contains(subString)) {
                throw new AssertException("'" + subString + "' not substring of '" + fullString + "'");
            }
        }

        public static void DoesNotContain(string subString, string fullString) {
            if (fullString.Contains(subString)) {
                throw new AssertException("'" + subString + "' is substring of '" + fullString + "'");
            }
        }

        public static void DoesNotContain<T>(T element, IEnumerable<T> elements) {
            if (elements.Contains(element)) {
                throw new AssertException("'" + element + "' is substring of '" + elements + "'");
            }
        }

    }

}