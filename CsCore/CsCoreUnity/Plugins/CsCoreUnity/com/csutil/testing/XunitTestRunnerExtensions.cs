using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.testing {

    public static class XunitTestRunnerExtensions {

        public static async Task RunTestsInClass(this Type classToTest, Action<XunitTestRunner.Test> onTestStarted, Func<XunitTestRunner.Test, bool> testBlacklistFilter = null) {
            using (var t = Log.MethodEnteredWith("classToTest: " + classToTest)) {
                IEnumerable<XunitTestRunner.Test> collectedTests = XunitTestRunner.GetIteratorOverAllTests(classToTest, onTestStarted);
                if (testBlacklistFilter != null) {
                    collectedTests = collectedTests.Filter(testBlacklistFilter);
                }
                await collectedTests.RunTests();
            }
        }

        public static async Task RunTests(this IEnumerable<XunitTestRunner.Test> self) {
            using (var t = Log.MethodEnteredWith("tests.count=" + self)) {
                var allTestsTasks = new List<Task>();
                foreach (var testToRun in self) {
                    try {
                        var testTask = testToRun.RunTest().WithTimeout(30000);
                        allTestsTasks.Add(testTask);
                        await testTask;
                    }
                    catch (Exception e) { Log.e(e); } // All task errors will be thrown below together 
                }
                await Task.WhenAll(allTestsTasks);
            }
        }

        public static async Task RunTest(this XunitTestRunner.Test self) {
            using (var t = Log.MethodEnteredWith("XUnit-Test: " + self)) {
                await TaskV2.Delay(100);
                self.StartTest();
                await self.testTask;
            }
            if (!self.testTask.IsCompletedSuccessfull()) {
                Log.w("Error in test " + self);
                await TaskV2.Delay(100);
                var ex = self.reportedError?.SourceException;
                if (ex == null) { ex = self.testTask.Exception; }
                Log.e("" + self, ex);
                await TaskV2.Delay(100);
                self.reportedError.Throw();
            }
        }

    }

}
