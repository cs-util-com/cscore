using com.csutil.testing;
using com.csutil.tests.http;
using com.csutil.tests.model;
using com.csutil.tests.model.immutable;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace com.csutil.tests {

    /// <summary> Tests the Assert class and other reimplemented Xunit classes </summary>
    public class XunitTestRunnerTests {

        [UnityTest]
        public IEnumerator RunXunitTest_DataStoreExample2() {
            yield return RunTestsInClass(typeof(DataStoreExample2)).AsCoroutine();
        }

        [UnityTest]
        public IEnumerator RunXunitTest_FeatureFlagTests() {
            yield return RunTestsInClass(typeof(FeatureFlagTests)).AsCoroutine();
        }

        [UnityTest]
        public IEnumerator RunXunitTest_RestTests() {
            yield return RunTestsInClass(typeof(RestTests)).AsCoroutine();
        }

        //[UnityTest]
        public IEnumerator RunAllXunitTest() {
            var allClasses = typeof(MathTests).Assembly.GetExportedTypes();
            foreach (var classToTest in allClasses) {
                yield return RunTestsInClass(classToTest).AsCoroutine();
            }
        }

        public static async Task RunTestsInClass(Type classToTest, Func<XunitTestRunner.Test, bool> testBlacklistFilter = null) {
            using (var t = Log.MethodEnteredWith("classToTest: " + classToTest)) {
                await classToTest.RunTestsInClass(delegate {
                    Log.instance = new LogForXunitTestRunnerInUnity(); // setup before each test
                }, testBlacklistFilter);
            }
        }

    }

}

