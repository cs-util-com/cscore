﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.async {

    class TaskV2WebGL : TaskV2 {

        protected override async Task DelayTask(int millisecondsDelay, CancellationToken cancellationToken) {
            if (!ApplicationV2.isPlaying) {
                await base.DelayTask(millisecondsDelay, cancellationToken);
            } else {
                cancellationToken.ThrowIfCancellationRequested();
                await StartCoroutineAsTask(DelayCoroutine(millisecondsDelay));
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private IEnumerator DelayCoroutine(int ms) { yield return new WaitForSeconds(ms / 1000f); }

        protected override Task RunTask(Action action) {
            return StartCoroutineAsTask(RunCoroutine(action));
        }

        private static Task StartCoroutineAsTask(IEnumerator iEnum) {
            AssertV3.IsTrue(ApplicationV2.isPlaying, () => "In EDIT mode!");
            var tcs = new TaskCompletionSource<bool>();
            MainThread.Invoke(() => { MainThread.instance.StartCoroutineAsTask(tcs, iEnum, () => true); });
            return tcs.Task;
        }

        private IEnumerator RunCoroutine(Action action) {
            yield return new WaitForEndOfFrame();
            action();
        }

        protected override Task RunTask(Func<Task> asyncAction) { return asyncAction(); }

        protected override Task<T> RunTask<T>(Func<Task<T>> asyncFunction) { return asyncFunction(); }

        protected override Task RunTask(Func<Task> asyncAction, CancellationTokenSource cancel, TaskScheduler scheduler) {
            return asyncAction();
        }

        protected override Task<T> RunTask<T>(Func<Task<T>> asyncFunction, CancellationTokenSource cancel, TaskScheduler scheduler) {
            return asyncFunction();
        }

    }

}
