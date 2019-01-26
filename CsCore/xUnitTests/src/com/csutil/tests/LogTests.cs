using System;
using System.Diagnostics;
using System.Threading;
using com.csutil.logging;
using Xunit;

namespace com.csutil.tests {

    public class LogTests {

        [Fact]
        public static void TestBasicLogOutputExamples() {
            Log.d("I'm a log message");
            Log.w("I'm a warning");
            Log.e("I'm an error");
            Log.e(new Exception("I'm an exception"));
            Log.w("I'm a warning with parmas:", "param 1", 2, "..");
        }


        [Fact]
        public static void TestLoggingMethodStartAndEnd() {
            SomeExampleMethod1("I am a string", 123);
        }

        // Logging when I method is entered and left:
        private static void SomeExampleMethod1(string s, int i) {
            Stopwatch timing = Log.MethodEntered("s=" + s, "i=" + i);
            { // .. here would be some method logic ..
                Thread.Sleep(1);
            } // .. as the last line in the tracked method add:
            Log.MethodDone(timing, maxAllowedTimeInMs: 50);
        }

        [Fact]
        public static void TestAssertV2Methods() {

            AssertV2.ThrowExeptionIfAssertionFails(() => {

                AssertV2.AreEqual(true, AssertV2.throwExeptionIfAssertionFails, "AssertV2.throwExeptionIfAssertionFails");

                AssertV2.IsTrue(1 + 1 == 2, "This assertion must not fail");
                AssertV2.Throws<Exception>(() => {
                    AssertV2.IsTrue(1 + 1 == 4, "This assertion has to fail");
                    Log.e("This line should never be printed since throwExeptionIfAssertionFails is true");
                });

                var s1 = "a";
                AssertV2.AreEqual(s1, s1);
                AssertV2.Throws<Exception>(() => { AssertV2.AreNotEqual(s1, s1, "s1"); });

                string myVarX = null;
                AssertV2.IsNull(null, "myVarX");
                myVarX = "Now myVarX is not null anymore";
                AssertV2.Throws<Exception>(() => { AssertV2.IsNull(myVarX, "myVarX"); });

                AssertV2.AreEqual(1, 1);
                AssertV2.Throws<Exception>(() => { AssertV2.AreEqual(1, 2); });

                AssertV2.AreNotEqual(1, 2);
                AssertV2.Throws<Exception>(() => { AssertV2.AreNotEqual(1, 1); });

                var stopWatch = AssertV2.TrackTiming();
                Thread.Sleep(10);
                stopWatch.Stop();
                AssertV2.Throws<Exception>(() => { stopWatch.AssertUnderXms(1); }); // This should always fail

                stopWatch.AssertUnderXms(50);
                AssertV2.IsTrue(stopWatch.IsUnderXms(50), "More time was needed than expected!");
            });

        }

        [Fact]
        public static void TestAssertV2Throws() {

            AssertV2.ThrowExeptionIfAssertionFails(() => {

                try {
                    AssertV2.Throws<Exception>(() => {
                        AssertV2.AreEqual(1, 1); // this will not fail..
                    }); // ..so the AssertV2.Throws should fail
                    Log.e("This line should never be reached since AssertV2.Throws should fail!");
                    throw new Exception("AssertV2.Throws did not fail correctly!");
                } catch (AssertV2.ThrowsException) { // Only catch it if its a ThrowsException
                                                     // AssertV2.Throws failed correctly and threw an ThrowsException error
                    Log.d("ThrowsException was expected and arrived correctly");
                }

            });

        }

    }

}