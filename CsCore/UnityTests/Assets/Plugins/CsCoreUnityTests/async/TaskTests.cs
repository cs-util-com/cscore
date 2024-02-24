﻿using System;
using NUnit.Framework;
using System.Collections;
using System.Threading;
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
        public IEnumerator TestMainThread3() {
            yield return TaskV2.Run(TestMainThread3Asnyc).AsCoroutine();
        }

        private async Task TestMainThread3Asnyc() {
            Assert.False(MainThread.isMainThread);
            bool wasCalled = false;
            Assert.True(await MainThread.instance.ExecuteOnMainThreadAsync(async () => {
                await TaskV2.Delay(100);
                Assert.True(MainThread.isMainThread);
                wasCalled = true;
                return true;
            }));
            Assert.True(wasCalled);
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

            Assert.AreEqual("abc", await MainThread.Invoke(async () => {
                await TaskV2.Delay(100);
                return "abc";
            }));
        }

        [UnityTest]
        public IEnumerator TestOrderOfMainThreadEvents() {
            yield return TestOrderOfMainThreadEventsAsync().AsCoroutine();
        }

        private async Task TestOrderOfMainThreadEventsAsync() {
            await TaskV2.Run(async () => {
                var i = 0;
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                Log.d("MainThread.Invoke");
                MainThread.Invoke(SlowOperationOnMainThread(i++));
                await MainThread.Invoke(() => {
                    MainThread.Invoke(SlowOperationOnMainThread(i++));
                    return Task.CompletedTask;
                });
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                await TaskV2.Run(() => MainThread.Invoke(SlowOperationOnMainThread(i++)));
                Log.d("MainThread.Invoke");
                MainThread.Invoke(SlowOperationOnMainThread(i++));
                await MainThread.Invoke(() => {
                    MainThread.Invoke(SlowOperationOnMainThread(i++));
                    return Task.CompletedTask;
                });
            });
        }

        private int currentI = -1;
        private Action SlowOperationOnMainThread(int i) {
            return () => {
                Thread.Sleep(250);
                Assert.AreEqual(currentI + 1, i, $"currentI={currentI} but i={i}");
                currentI = i;
                Log.d("currentI=" + currentI);
            };
        }

    }

}