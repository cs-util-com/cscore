using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.csutil;

namespace com.csutil.injection.tests {

    public class TestUiInjection {

        [Test]
        public void TestSingleton() {
            var singletonInstance = new MySingleton();
            IoC.inject.SetSingleton(singletonInstance);

            var a = IoC.inject.Get<MySingleton>(this);
            var b = IoC.inject.Get<MySingleton>(this);
            Assert.AreEqual(a, b);
            Assert.AreEqual(singletonInstance, a);
        }

        [UnityTest]
        public IEnumerator Test1WithEnumeratorPasses() {
            yield return null;
        }

        private class MySingleton { }
    }

}
