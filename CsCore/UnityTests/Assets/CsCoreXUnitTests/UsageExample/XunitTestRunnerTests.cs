using com.csutil.logging;
using com.csutil.testing;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Xunit.Abstractions;

namespace com.csutil.tests {

    /// <summary> Tests the Assert class and other reimplemented Xunit classes </summary>
    public class XunitTestRunnerTests {

        [UnityTest]
        public IEnumerator ExampleUsage1() {
            XunitTestRunner x = new XunitTestRunner();
            var classToTest = typeof(MathTests);
            var tests = XunitTestRunner.RunTestsOnClass(classToTest);
            foreach (var test in tests) {
                yield return test.AsCoroutine();
                if (test.Result.testFailed) {
                    Log.e("" + test.Result, test.Result.reportedError.SourceException);
                    Assert.Fail("" + test.Result.reportedError.SourceException);
                }
            }
        }

    }

}
