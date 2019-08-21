using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xunit {

    public class Assert {

        public static void True(bool b, string msg = null) {
            if (!b) { throw (msg != null) ? new Exception(msg) : new Exception(); }
        }

        public static void False(bool b, string msg = null) { True(!b, msg); }

        public static void Null<T>(T obj) { True(null == obj, "Was NOT null: " + obj); }
        public static void NotNull<T>(T obj) { True(null != obj, "Was null: " + typeof(T)); }

        public static void Equal(object objA, object objB) {
            True(Equals(objA, objB), "NOT Equal: \n " + objA + " \n and \n " + objB);
        }
        public static void NotEqual(object objA, object objB) {
            True(!Equals(objA, objB), "EQUAL: \n " + objA + " \n and \n " + objB);
        }

        public static void IsType<T>(object obj) where T : class {
            True((obj as T) != null, "Not Type " + typeof(T) + ": " + obj);
        }

        public static void Throws<T>(Action a) where T : Exception {
            NotEqual(typeof(Exception), typeof(T)); // Must be subtype of Exception
            try { a(); throw new Exception("Did not throw " + typeof(T)); } catch (T) { }
        }

        public static async Task ThrowsAsync<T>(Func<Task> a) where T : Exception {
            NotEqual(typeof(Exception), typeof(T)); // Must be subtype of Exception
            try { await a(); throw new Exception("Did not throw " + typeof(T)); } catch (T) { }
        }

        public static void Same<T>(T objA, T objB) {
            True(ReferenceEquals(objA, objB));
        }
        public static void NotSame<T>(T objA, T objB) {
            True(!ReferenceEquals(objA, objB));
        }

        public static void InRange<T>(T obj, T min, T max) where T : IComparable {
            True(min.CompareTo(obj) <= 0 && obj.CompareTo(max) <= 0);
        }

        public static void Single<T>(IEnumerable<T> e) { Equal(1, e.Count()); }

        public static void Empty<T>(IEnumerable<T> e) { Equal(0, e.Count()); }
        public static void NotEmpty<T>(IEnumerable<T> e) { NotEqual(0, e.Count()); }

        public static void Contains(object obj, IEnumerable e) {
            foreach (var i in e) { if (Equals(obj, i)) { return; } }
            throw new Exception("Not found in " + e + ":" + obj);
        }

    }

}