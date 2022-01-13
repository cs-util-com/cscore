using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.threading {

    class TaskTests {

        [UnityTest]
        public IEnumerator TestMainThread2() {
            Assert.IsTrue(MainThread.instance.enabled);
            MainThread.Invoke(() => { Assert.IsTrue(Application.isPlaying); });
            yield return null;
        }

    }

}