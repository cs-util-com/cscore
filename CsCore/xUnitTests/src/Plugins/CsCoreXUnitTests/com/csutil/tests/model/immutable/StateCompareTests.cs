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

    }

}