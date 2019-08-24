using com.csutil.logging;
using com.csutil.testing;
using com.csutil.tests.model.immutable;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests {

    /// <summary> Tests the Assert class and other reimplemented Xunit classes </summary>
    public class XunitTestRunnerTests {

        [UnityTest]
        public IEnumerator ExampleUsage1() {

            //yield return RunTestsInClass(typeof(DataStoreExample2));

            var allClasses = typeof(MathTests).Assembly.GetExportedTypes();
            foreach (var classToTest in allClasses) {
                yield return RunTestsInClass(classToTest);
            }
        }

        private IEnumerator RunTestsInClass(Type classToTest) {
            var runningTests = XunitTestRunner.RunTestsOnClass(classToTest);
            foreach (var runningTest in runningTests) { yield return LogTest(runningTest); }
        }

        private IEnumerator LogTest(XunitTestRunner.Test runningTest) {
            var t = Log.MethodEntered("Now running test " + runningTest);
            yield return new WaitForSeconds(0.1f);
            yield return runningTest.testTask.AsCoroutine((e) => { Debug.LogWarning(e); }, timeoutInMs: 60000);
            Log.MethodDone(t);
            if (runningTest.testFailed) {
                Debug.LogWarning("Error in test " + runningTest);
                yield return new WaitForSeconds(0.1f);
                Log.w("" + runningTest, runningTest.reportedError.SourceException);
                Debug.LogError(runningTest.reportedError.SourceException);
                yield return new WaitForSeconds(0.1f);
                runningTest.reportedError.Throw();
            }
        }
    }

}
