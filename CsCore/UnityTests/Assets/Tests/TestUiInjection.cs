using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.csutil;
using com.csutil.http;
using System;

namespace com.csutil.injection.tests {

    public class TestUiInjection {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestSingleton() {
            var singletonInstance = new MySingleton();
            IoC.inject.SetSingleton(singletonInstance);

            var a = IoC.inject.Get<MySingleton>(this);
            var b = IoC.inject.Get<MySingleton>(this);
            Assert.AreEqual(a, b);
            Assert.AreEqual(singletonInstance, a);
        }

        private class MySingleton { }

        [UnityTest]
        public IEnumerator Test1WithEnumeratorPasses() {

            // It should not be possible to create a mono via default constructor:
            AssertV2.Throws<Exception>(() => { IoC.inject.GetOrAddSingleton<WebRequestRunner>(this); });

            var singletonsName = "SingletonsMaster1";
            var x = IoC.inject.GetOrAddComponentSingleton<WebRequestRunner>(this, true, singletonsName);
            Assert.NotNull(x);
            var go = x.gameObject;
            Assert.NotNull(go);
            Assert.AreEqual(singletonsName, go.GetParent().name);

            yield return null;
        }





    }

}
