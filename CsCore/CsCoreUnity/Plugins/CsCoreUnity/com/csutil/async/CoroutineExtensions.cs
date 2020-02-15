using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public static class CoroutineExtensions {

        public static IEnumerator AsCoroutine(this Task self, int timeoutInMs = -1, int waitIntervalInMs = 20) {
            Action<Exception> defaultOnErrorAction = (e) => { throw e; };
            yield return AsCoroutine(self, defaultOnErrorAction, timeoutInMs, waitIntervalInMs);
        }

        public static IEnumerator AsCoroutine(this Task self, Action<Exception> onError, int timeoutInMs = -1, int waitIntervalInMs = 20) {
            var waitIntervalBeforeNextCheck = new WaitForSeconds(waitIntervalInMs / 1000f);
            Stopwatch timer = timeoutInMs > 0 ? Stopwatch.StartNew() : null;
            while (!self.IsCompleted) {
                yield return waitIntervalBeforeNextCheck;
                AssertV2.IsTrue(self.Status != TaskStatus.WaitingToRun, "Task is WaitingToRun");
                if (timer != null && timeoutInMs < timer.ElapsedMilliseconds) {
                    onError(new TimeoutException("Task timeout after " + timer.ElapsedMilliseconds + "ms"));
                    break;
                }
            }
            if (self.IsFaulted) { onError.InvokeIfNotNull(self.Exception); }
            yield return null;
        }

        public static IEnumerator AsCoroutine(this TaskRunner.MonitoredTask self, int timeoutInMs = -1, int waitIntervalInMs = 20) {
            return self.task.AsCoroutine(timeoutInMs, waitIntervalInMs);
        }

        public static IEnumerator AsCoroutine<T>(this TaskRunner.MonitoredTask<T> self, int timeoutInMs = -1, int waitIntervalInMs = 20) {
            return self.task.AsCoroutine(timeoutInMs, waitIntervalInMs);
        }

        public static Coroutine ExecuteRepeated(this MonoBehaviour self, Func<bool> task,
            int delayInMsBetweenIterations, int delayInMsBeforeFirstExecution = 0, float repetitions = -1) {
            if (!self.isActiveAndEnabled) { throw new Exception("ExecuteRepeated called on inactive mono"); }
            return self.StartCoroutine(ExecuteRepeated(task, self, delayInMsBetweenIterations, delayInMsBeforeFirstExecution, repetitions));
        }

        private static IEnumerator ExecuteRepeated(Func<bool> task, MonoBehaviour mono, int repeatDelayInMs, int firstDelayInMs = 0, float rep = -1) {
            if (firstDelayInMs > 0) { yield return new WaitForSeconds(firstDelayInMs / 1000f); }
            var waitTask = new WaitForSeconds(repeatDelayInMs / 1000f);
            while (rep != 0) {
                if (mono.enabled) { // pause the repeating task while the parent mono is disabled
                    if (!Run(task)) { break; }
                    rep--;
                }
                yield return waitTask;
            }
        }

        private static bool Run(Func<bool> t) { try { return t(); } catch (Exception e) { Log.e(e); } return false; }

        public static Coroutine ExecuteDelayed(this MonoBehaviour self, Action task, int delayInMsBeforeExecution = 0) {
            if (!self.isActiveAndEnabled) { throw new Exception("ExecuteDelayed called on inactive mono"); }
            return self.StartCoroutine(ExecuteDelayed(task, delayInMsBeforeExecution));
        }

        private static IEnumerator ExecuteDelayed(Action task, int delayMs) {
            if (delayMs > 0) { yield return new WaitForSeconds(delayMs / 1000f); } else { yield return new WaitForEndOfFrame(); }
            try { task(); } catch (Exception e) { Log.e(e); }
        }

        public static IEnumerator StartCoroutinesSequetially(this MonoBehaviour self, params Func<IEnumerator>[] tasks) {
            return StartCoroutinesSequetially(self, (IEnumerable<Func<IEnumerator>>)tasks);
        }

        public static IEnumerator StartCoroutinesSequetially(this MonoBehaviour self, IEnumerable<Func<IEnumerator>> tasks) {
            foreach (var task in tasks) { yield return self.StartCoroutine(task()); }
        }

        public static List<Coroutine> StartCoroutinesInParallel(this MonoBehaviour self, params Func<IEnumerator>[] tasks) {
            return StartCoroutinesInParallel(self, (IEnumerable<Func<IEnumerator>>)tasks);
        }

        public static List<Coroutine> StartCoroutinesInParallel(this MonoBehaviour self, IEnumerable<Func<IEnumerator>> tasks) {
            var r = new List<Coroutine>();
            foreach (var task in tasks) { r.Add(self.StartCoroutine(task())); }
            return r;
        }

        public static IEnumerator WaitForRunningCoroutinesToFinish(this IEnumerable<Coroutine> self) {
            foreach (var c in self) { yield return c; }
        }

        public static Task StartCoroutineAsTask(this MonoBehaviour self, IEnumerator routine) {
            return StartCoroutineAsTask(self, routine, () => true);
        }

        public static Task<T> StartCoroutineAsTask<T>(this MonoBehaviour self, IEnumerator routine, Func<T> onRoutineDone) {
            var tcs = new TaskCompletionSource<T>();
            self.StartCoroutineAsTask(tcs, routine, onRoutineDone);
            return tcs.Task;
        }

        public static void StartCoroutineAsTask<T>(this MonoBehaviour self, TaskCompletionSource<T> tcs, IEnumerator routine, Func<T> onRoutineDone) {
            self.StartCoroutine(routine.WithErrorCatch((e) => {
                if (e != null) { tcs.TrySetException(e); return; }
                try { tcs.SetResult(onRoutineDone()); }
                catch (Exception onDoneError) { tcs.TrySetException(onDoneError); }
            }));
        }

        public static IEnumerator WithErrorCatch(this IEnumerator coroutineToWrap, Action<Exception> onRoutineDone) {
            while (true) {
                object current;
                try {
                    if (!coroutineToWrap.MoveNext()) { break; }
                    current = coroutineToWrap.Current;
                }
                catch (Exception e) { onRoutineDone(e); yield break; }
                yield return current;
            }
            onRoutineDone(null);
        }

    }

}
