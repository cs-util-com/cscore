using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.eventbus {

    public class LoggingTests : UnitTestMono {

        public override IEnumerator RunTest() {

            yield return new WaitForSeconds(2);

            var t = Log.MethodEntered();
            yield return Method1().AsCoroutine();
            Method2();
            Log.MethodDone(t);

        }

        private async Task Method1() {
            var t = Log.MethodEntered();
            await TaskV2.Delay(10);
            Method2();
            await Method3();
            Log.MethodDone(t);
        }

        private void Method2() {
            var t = Log.MethodEntered();
            ExpensiveCalculation();
            Log.MethodDone(t);
        }

        private int ExpensiveCalculation() {
            var t = Log.MethodEntered();
            var sum = 0; for (int i = 0; i < 50000000; i++) { sum += i; }
            Log.MethodDone(t);
            return sum;
        }

        private async Task Method3() {
            var t = Log.MethodEntered();
            Method2();
            ExpensiveCalculation();
            await TaskV2.Delay(10);
            Method2();
            Log.MethodDone(t);
        }

    }

}