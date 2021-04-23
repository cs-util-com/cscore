using Xunit;

namespace com.csutil.tests {

    public class GotoExample {

        public GotoExample(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void Example1() {
            var i = 0;
            StartOfMyLoop:
            i++;
            if (i < 5) { goto StartOfMyLoop; }
            Assert.Equal(5, i);
        }

    }

}