using Do = System.Diagnostics.Debug;
using Xunit;

namespace com.csutil.tests {

    public class UsingWithRenameExample {

        public UsingWithRenameExample(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void Example1() {
            // In this file the System.Diagnostics.Debug class can be used as 'Do':
            Do.Assert(3 == 1 + 2);
        }

    }

}