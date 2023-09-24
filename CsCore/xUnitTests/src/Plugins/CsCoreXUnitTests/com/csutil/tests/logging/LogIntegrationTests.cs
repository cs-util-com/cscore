using System;
using Xunit;

namespace com.csutil.integrationTests {
    
    public class LogIntegrationTests {
        
        [Fact]
        public void TestUsingAssertV2ForPrintDebugging() {
            AssertV3.SetupPrintDebuggingSuccessfulAssertions(); // Turn on print debugging
            // Then call AssertV2 methods that normally do not produce any log output (because they dont fail):
            TestAssertV2Methods();
            AssertV3.onAssertSuccess = null; // Turn off print debugging again
        }
        
        [Fact]
        public void TestAssertV2Methods() {

            AssertV3.ThrowExeptionIfAssertionFails(() => {
                AssertV3.IsTrue(1 + 1 == 2, () => "This assertion must not fail");
                var s1 = "a";
                AssertV3.AreEqual(s1, s1);
                AssertV3.IsNull(null, "myVarX");
                AssertV3.AreEqual(1, 1);
                AssertV3.AreNotEqual(1, 2);
            });

            var stopWatch = AssertV3.TrackTiming();
            var res = 1f;
            for (float i = 1; i < 500000; i++) { res = i / res + i; }
            Assert.NotEqual(0, res);

            stopWatch.Stop();
            AssertV3.ThrowExeptionIfAssertionFails(() => { stopWatch.AssertUnderXms(200); });
            Assert.True(stopWatch.IsUnderXms(200), "More time was needed than expected!");

        }
        
        [Fact]
        public void TestAssertV2Throws() {

            AssertV3.ThrowExeptionIfAssertionFails(() => {

                try {
                    AssertV3.Throws<Exception>(() => {
                        AssertV3.AreEqual(1, 1); // this will not fail..
                    }); // ..so the AssertV2.Throws should fail
                    Log.e("This line should never be reached since AssertV2.Throws should fail!");
                    throw new Exception("AssertV2.Throws did not fail correctly!");
                } catch (AssertV3.ThrowsException) { // Only catch it if its a ThrowsException
                    // AssertV2.Throws failed correctly and threw an ThrowsException error
                    Log.d("ThrowsException was expected and arrived correctly");
                }

            });

        }
        
    }
    
}