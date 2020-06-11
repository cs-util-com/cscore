using Xunit;

namespace com.csutil.tests.extensions {

    public class EnumTests {

        public EnumTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        private enum MyEnum123 { state1, state2 }

        [Fact]
        public void TestParseAndTryParse() {
            Assert.Equal(MyEnum123.state1, EnumUtil.Parse<MyEnum123>("state1"));
            Assert.Equal(MyEnum123.state1, EnumUtil.Parse<MyEnum123>("state 1"));

            Assert.Equal(MyEnum123.state1, EnumUtil.TryParse("state1", MyEnum123.state2));
            Assert.Equal(MyEnum123.state1, EnumUtil.TryParse("state 1", MyEnum123.state2));
        }

        [Fact]
        public void TestGetName() {
            Assert.Equal("state1", EnumUtil.GetName(MyEnum123.state1));
            Assert.Equal("state2", EnumUtil.GetName(MyEnum123.state2));
        }

        [Fact]
        public void TestIsEnum() {
            Assert.True(EnumUtil.IsEnum<MyEnum123>());
            Assert.False(EnumUtil.IsEnum<string>());
        }

        [Fact]
        public void TestTryParseOut() {
            {
                if (EnumUtil.TryParse("state1", out MyEnum123 x)) {
                    Assert.Equal(MyEnum123.state1, x);
                } else { throw Log.e("Try Parse failed"); }
            }
            {
                if (EnumUtil.TryParse("state 1", out MyEnum123 x)) {
                    Assert.Equal(MyEnum123.state1, x);
                } else { throw Log.e("Try Parse failed"); }
            }
        }

        [Fact]
        public void TestContainsFlag() {
            MyEnum123 myStateMachine = MyEnum123.state1;
            Assert.False(myStateMachine.ContainsFlag(MyEnum123.state2));

            myStateMachine = MyEnum123.state1 | MyEnum123.state2;
            Assert.True(myStateMachine.ContainsFlag(MyEnum123.state2));

            myStateMachine = MyEnum123.state2;
            Assert.True(myStateMachine.ContainsFlag(MyEnum123.state2));
        }

    }

}