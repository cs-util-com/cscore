using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.csutil;
using com.csutil.http;
using System;

namespace com.csutil.injection.tests {

    public class TestComponentInjection {

        private class MySingleton { }

        [SetUp]
        public void BeforeEachTest() { }

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

        [Test]
        public void TestGetOrAddComponentSingleton() {
            IoC.inject.RemoveAllInjectorsFor<WebRequestRunner>();
            // It should not be possible to create a mono via default constructor:
            AssertV2.Throws<Exception>(() => {
                IoC.inject.GetOrAddSingleton<WebRequestRunner>(this);
            });
            {
                var singletonsName = "SingletonsMaster1";
                var x1 = IoC.inject.GetOrAddComponentSingleton<WebRequestRunner>(this, singletonsName);
                Assert.NotNull(x1);
                var go = x1.gameObject;
                Assert.NotNull(go);
                Assert.AreEqual(singletonsName, go.GetParent().name);

                var x2 = IoC.inject.Get<WebRequestRunner>(this);
                Assert.AreEqual(x1, x2);
                Assert.AreEqual(x1.gameObject, x2.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator TestAsync1() {
            yield return null;
        }





    }

}
