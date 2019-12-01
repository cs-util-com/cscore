using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.logging;
using Xunit;

namespace com.csutil.tests {

    public class LogTests {

        public LogTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestBasicLogOutputExamples() {
            Log.d("I'm a log message");
            Log.w("I'm a warning");
            Log.e("I'm an error");
            Log.e(new Exception("I'm an exception"));
            Log.w("I'm a warning with parmas:", "param 1", 2, "..");
        }

        [Fact]
        public void TestLoggingMethodStartOnly() {
            Stopwatch s = Log.MethodEntered("TestLoggingMethodStartOnly", "abc", 123);
            // ...
            s.AssertUnderXms(80);
        }

        [Fact]
        public void TestLoggingMethodEndOnly() {
            Stopwatch s = Stopwatch.StartNew();
            // ...
            Log.MethodDone(s);
        }

        [Fact]
        public void TestLoggingMethodEndOnly2() {
            var s = StopwatchV2.StartNewV2("TestLoggingMethodEndOnly2");
            // ...
            Log.MethodDone(s);
        }

        [Fact]
        public async Task TestLoggingMethodStartAndEnd() {
            await SomeExampleMethod1("I am a string", 123);
        }

        // Logging when I method is entered and left:
        private async Task SomeExampleMethod1(string s, int i) {
            Stopwatch timing = Log.MethodEntered("SomeExampleMethod1", "s =" + s, "i=" + i);
            { // .. here would be some method logic ..
                Log.d("Will now work for 5 ms");
                await TaskV2.Delay(5);
                Log.d("worked for 5 ms");
            } // .. as the last line in the tracked method add:
            Log.MethodDone(timing, maxAllowedTimeInMs: 5000);
        }

        [Fact]
        public void TestAppFlowTracking() {
            AppFlow.instance = new MyAppFlowTracker1();
            Log.MethodEntered(); // This will internally notify the AppFlow instance
            Assert.True((AppFlow.instance as MyAppFlowTracker1).wasCalledByTestAppFlowTrackingTest);
        }

        private class MyAppFlowTracker1 : IAppFlow {
            public bool wasCalledByTestAppFlowTrackingTest = false;
            public void TrackEvent(string category, string action, object[] args) {
                if ("TestAppFlowTracking" == action) { wasCalledByTestAppFlowTrackingTest = true; }
            }
        }

        [Fact]
        public void TestAssertV2Methods() {

            AssertV2.ThrowExeptionIfAssertionFails(() => {
                AssertV2.IsTrue(1 + 1 == 2, "This assertion must not fail");
                var s1 = "a";
                AssertV2.AreEqual(s1, s1);
                AssertV2.IsNull(null, "myVarX");
                AssertV2.AreEqual(1, 1);
                AssertV2.AreNotEqual(1, 2);
            });

            var stopWatch = AssertV2.TrackTiming();
            var res = 1f;
            for (float i = 1; i < 500000; i++) { res = i / res + i; }
            Assert.NotEqual(0, res);

            stopWatch.Stop();
            AssertV2.ThrowExeptionIfAssertionFails(() => { stopWatch.AssertUnderXms(200); });
            Assert.True(stopWatch.IsUnderXms(200), "More time was needed than expected!");

        }

        [Fact]
        public void TestAssertV2Throws() {

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

        [Fact]
        public void TestLoggingToFile() {
            var targetFileToLogInto = EnvironmentV2.instance.GetOrAddTempFolder("TestLoggingToFile").GetChild("log.txt");
            targetFileToLogInto.DeleteV2();

            ILog fileLogger = new LogToFile(targetFileToLogInto);
            var logText = "!! Test LogDebug 123 !!";
            fileLogger.LogDebug(logText);
            SendSomeEventsToLog(fileLogger);
            LogToFile.LogStructure logStructure = LogToFile.LoadLogFile(targetFileToLogInto);
            Assert.NotNull(logStructure);
            List<LogToFile.LogEntry> entries = logStructure.logEntries;
            Assert.Equal(5, entries.Count);
            Assert.Contains(logText, entries.First().d);
        }

        [Fact]
        public void TestLoggingToMultipleLoggers() {
            var targetFileToLogInto = EnvironmentV2.instance.GetOrAddTempFolder("TestLoggingToMultipleLoggers").GetChild("log.txt");
            targetFileToLogInto.DeleteV2();
            var multiLogger = new LogToMultipleLoggers();
            multiLogger.loggers.Add(new LogToConsole());
            multiLogger.loggers.Add(new LogToFile(targetFileToLogInto));
            var mockLog = new LogToTestMock();
            multiLogger.loggers.Add(mockLog);

            SendSomeEventsToLog(multiLogger);
            mockLog.AssertAllMethodsOfMockLogWereCalled();

            LogToFile.LogStructure logStructure = LogToFile.LoadLogFile(targetFileToLogInto);
            Assert.Equal(4, logStructure.logEntries.Count);
        }

        [Fact]
        public void TestLogToTestMock() {
            SendSomeEventsToLog(new LogToTestMock());
        }

        private static void SendSomeEventsToLog(ILog log) {
            log.LogDebug("Test LogDebug");
            log.LogWarning("Test LogWarning");
            Assert.NotNull(log.LogError("Test LogError"));
            var e = new Exception("Test LogExeption");
            Assert.Same(e, log.LogExeption(e));
            if (log is LogToTestMock mockLog) { mockLog.AssertAllMethodsOfMockLogWereCalled(); }
        }

        private class LogToTestMock : LogDefaultImpl {
            private string latestLog;
            private string latestWarning;
            private string latestError;

            protected override void PrintDebugMessage(string l, object[] args) { this.latestLog = l; }
            protected override void PrintErrorMessage(string e, object[] args) { this.latestError = e; }
            protected override void PrintWarningMessage(string w, object[] args) { this.latestWarning = w; }

            internal void AssertAllMethodsOfMockLogWereCalled() {
                Assert.NotEmpty(latestLog);
                Assert.NotEmpty(latestWarning);
                Assert.NotEmpty(latestError);
            }
        }

    }

}