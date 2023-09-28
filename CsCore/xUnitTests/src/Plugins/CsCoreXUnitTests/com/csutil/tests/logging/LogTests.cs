using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.logging;
using Xunit;

namespace com.csutil.tests {

    public class LogTests {

        public LogTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        void TestBasicLogOutputExamples() { InnerMethod1(); }
        void InnerMethod1() { InnerMethod2(); }
        void InnerMethod2() { InnerMethod3(); }
        void InnerMethod3() {
            Log.d("I'm a log message");
            Log.i("I'm an info message");
            Log.w("I'm a warning");
            Log.e("I'm an error");
            Log.e(new Exception("I'm an exception"));
            Log.w("I'm a warning with params:", "param 1", 2, "..");
            MyMethod1();
        }
        void MyMethod1() {
            using StopwatchV2 timing = Log.MethodEntered();
            // Some method body (duration and memory will be logged)
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

        

        /// <summary> Prints out failing AssertV2.AreEqual to show if they are readable in the log </summary>
        [Fact]
        public void TestReadableAssertAreEqualErrorOutputs() {
            AssertV3.AreEqual("abcd", "abce");
            AssertV3.AreEqual(new int[4] { 1, 2, 2, 4 }, new int[4] { 1, 2, 3, 4 });
            AssertV3.AreEqual(new int[2] { 1, 2 }, new int[2] { 1, 3 });
            AssertV3.AreEqual(new int[6] { 1, 2, 3, 4, 5, 6 }, new int[6] { 1, 2, 3, 4, 5, 7 });
            AssertV3.AreEqual(new int[2] { 1, 2 }, new int[1] { 1 });
            AssertV3.AreEqual(new int[1] { 1 }, new int[2] { 1, 2 });
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

        [Fact]
        public void TestLogSystemAttributes() {
            var sysInfo = EnvironmentV2.instance.systemInfo;
            Log.d("SysInfos: " + JsonWriter.AsPrettyString(sysInfo));
            var c = TimeZoneInfo.Local.GetUtcOffset(DateTimeV2.UtcNow);
            Log.d("TimeZoneInfo.Local.GetUtcOffset(DateTimeV2.UtcNow): " + JsonWriter.AsPrettyString(c));
            var si2 = CloneHelper.DeepCopyViaJson(sysInfo);
            Assert.Equal(JsonWriter.AsPrettyString(sysInfo), JsonWriter.AsPrettyString(si2));
        }

        [Fact]
        public async Task TestThrowNullExtension() {
            object x = null;
            Assert.Throws<ArgumentNullException>(() => {
                x.ThrowErrorIfNull("x");
            });
            Assert.Throws<InvalidDataException>(() => {
                x.ThrowErrorIfNull(() => new InvalidDataException("x was null"));
            });
            await Assert.ThrowsAsync<InvalidDataException>(async () => {
                await x.ThrowErrorIfNull(async () => new InvalidDataException("x was null"));
            });
        }

        [Fact]
        public void ExampleOfExceptionInfoInMethodLogging() {
            try { // Check the log output after running this test
                using (Log.MethodEntered()) {
                    throw new Exception(); // Simulate error during the method
                }
            } catch (Exception) { }
        }

        private class LogToTestMock : LogDefaultImpl {
            private string latestLog;
            private string latestWarning;
            private string latestError;

            protected override void PrintDebugMessage(string l, params object[] args) { this.latestLog = l; }
            protected override void PrintInfoMessage(string i, params object[] args) { this.latestLog = i; }
            protected override void PrintErrorMessage(string e, params object[] args) { this.latestError = e; }
            protected override void PrintWarningMessage(string w, params object[] args) { this.latestWarning = w; }

            internal void AssertAllMethodsOfMockLogWereCalled() {
                Assert.NotEmpty(latestLog);
                Assert.NotEmpty(latestWarning);
                Assert.NotEmpty(latestError);
            }
        }

    }

}