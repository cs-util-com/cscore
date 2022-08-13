using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
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

        [UnityTest]
        public IEnumerator TestAsyncTaskOnMainThread() {
            yield return TestAsyncTaskOnMainThreadAsync().AsCoroutine();
        }

        private async Task TestAsyncTaskOnMainThreadAsync() {
            var b1 = false;
            await MainThread.Invoke(async () => {
                await TaskV2.Delay(100);
                b1 = true;
            });
            Assert.True(b1);

            Assert.AreEqual("abc", await MainThread.Invoke(async () => "abc"));
        }

    }

}