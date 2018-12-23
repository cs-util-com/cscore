using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil {

    class TestCoroutines {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [UnityTest]
        public IEnumerator TestResultCallback() {

            var runner = new GameObject().GetOrAddComponent<CoroutineRunner>();
            runner.StartCoroutine(coroutineA());

            yield return null;
        }

        private IEnumerator coroutineA() {
            throw new NotImplementedException();
        }
    }

}
