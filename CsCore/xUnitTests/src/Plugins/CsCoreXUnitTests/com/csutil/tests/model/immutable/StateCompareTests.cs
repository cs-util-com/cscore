using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {
    public class StateCompareTests {

        public StateCompareTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestStateCompare1() {

            Assert.False(StateCompare.WasModified(1, 1));
            Assert.True(StateCompare.WasModified(1, 2));

            Assert.False(StateCompare.WasModified("1", "1"));
            Assert.True(StateCompare.WasModified("1", "2"));

            Assert.False(StateCompare.WasModified(null, (object)null));
            Assert.True(StateCompare.WasModified(null, new object()));

            var o1 = new object();
            var o2 = new object();
            Assert.False(StateCompare.WasModified(o1, o1));
            Assert.True(StateCompare.WasModified(o1, o2)); // Not same ref/pointer

        }

        [Fact]
        public void TestStateCompareNullable() {
            int? o1 = null;
            int? o2 = null;
            Assert.False(StateCompare.WasModified(o1, o2));
            o1 = 1;
            o2 = 1;
            Assert.False(StateCompare.WasModified(o1, o2));
            o1 = null;
            o2 = 1;
            Assert.True(StateCompare.WasModified(o1, o2));
            o1 = 1;
            o2 = null;
            Assert.True(StateCompare.WasModified(o1, o2));
            o1 = 1;
            o2 = 2;
            Assert.True(StateCompare.WasModified(o1, o2));
        }

        [Fact]
        public void TestStateCompareEnumerable() {
            var a = new MyClass() { s = "a" };
            var b1 = new MyClass() { s = "b" };
            var b2 = new MyClass() { s = "b" };

            var l1 = new List<MyClass>() { a, b1 };
            var l2_1 = new List<MyClass>() { a, b1 };
            var l2_2 = new List<MyClass>() { a, b2 };
            Assert.False(StateCompare.WasModified(l1, l1));
            Assert.True(StateCompare.WasModified(l1, l2_1));
            // 2 different arrays are created from the same source list:
            Assert.True(StateCompare.WasModified(l1.ToArray(), l1.ToArray()));

            // Same object references in both arrays (but arrays dont have same ref):
            Assert.True(l1.ToArray().SequenceEqual(l1.ToArray()));
            Assert.True(l1.ToArray().SequenceEqual(l2_1.ToArray()));
            // Because MyClass implements equals these are also equal:
            Assert.True(l1.ToArray().SequenceEqual(l2_2.ToArray()));
            Assert.False(l1.ToArray().Equals(l2_2.ToArray())); // Array ref not the same

            Assert.True(l1.ToArray().SequenceReferencesEqual(l2_1.ToArray()));
            Assert.False(l1.ToArray().SequenceReferencesEqual(l2_2.ToArray()));
            Assert.False(l1.ToArray().SequenceReferencesEqual(new MyClass[0]));
            Assert.False(new MyClass[0].SequenceReferencesEqual(l2_2.ToArray()));
        }

        private class MyClass {

            public string s;

            protected bool Equals(MyClass other) { return s == other.s; }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MyClass)obj);
            }

            public override int GetHashCode() { return (s != null ? s.GetHashCode() : 0); }

        }

    }

}