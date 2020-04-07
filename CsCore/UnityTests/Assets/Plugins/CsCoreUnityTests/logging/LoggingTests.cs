using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.eventbus {

    /// <summary>
    /// This class simulates a complex async calculation happening splitted up into 
    /// multiple methods that all use Log.MethodEntered() and Log.MethodDone() which
    /// cases the Unity profiler to automatically track these methods as separate steps
    /// in the Unity Profiler UI to allow easier debugging. 
    /// </summary>
    public class LoggingTests {

        [UnityTest]
        public IEnumerator RunTest() {
            Assert.IsTrue(MainThread.isMainThread);
            yield return new WaitForSeconds(2);

            var t = Log.MethodEntered();
            yield return Method1().AsCoroutine();
            Method2();
            Log.MethodDone(t);

        }

        private async Task Method1() {
            var t = Log.MethodEntered();
            Assert.IsTrue(MainThread.isMainThread);
            await TaskV2.Delay(10);
            Method2();
            await Method3();
            Log.MethodDone(t);
        }

        private void Method2() {
            var t = Log.MethodEntered();
            Assert.IsTrue(MainThread.isMainThread);
            ExpensiveCalculation();
            Log.MethodDone(t);
        }

        private int ExpensiveCalculation() {
            var t = Log.MethodEntered();
            Assert.IsTrue(MainThread.isMainThread);
            var sum = 0; for (int i = 0; i < 50000000; i++) { sum += i; }
            Log.MethodDone(t);
            return sum;
        }

        private async Task Method3() {
            var t = Log.MethodEntered();
            Assert.IsTrue(MainThread.isMainThread);
            Method2();
            ExpensiveCalculation();
            await TaskV2.Delay(10);
            Method2();
            Log.MethodDone(t);
        }

    }

}