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
            var waitIntervalBeforeNextCheck = new WaitForSeconds(waitIntervalInMs);
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
            float delayInSecBetweenIterations, float delayInSecBeforeFirstExecution = 0, float repetitions = -1) {
            if (!self.isActiveAndEnabled) { throw new Exception("ExecuteRepeated called on inactive mono"); }
            return self.StartCoroutine(ExecuteRepeated(task, self, delayInSecBetweenIterations, delayInSecBeforeFirstExecution, repetitions));
        }

        private static IEnumerator ExecuteRepeated(Func<bool> task, MonoBehaviour mono, float repeatDelay, float firstDelay = 0, float rep = -1) {
            if (firstDelay > 0) { yield return new WaitForSeconds(firstDelay); }
            var waitTask = new WaitForSeconds(repeatDelay);
            while (rep != 0) {
                if (mono.enabled) { // pause the repeating task while the parent mono is disabled
                    if (!Run(task)) { break; }
                    rep--;
                }
                yield return waitTask;
            }
        }

        private static bool Run(Func<bool> t) { try { return t(); } catch (Exception e) { Log.e(e); } return false; }

        public static Coroutine ExecuteDelayed(this MonoBehaviour self, Action task, float delayInSecBeforeExecution = 0f) {
            if (!self.isActiveAndEnabled) { throw new Exception("ExecuteDelayed called on inactive mono"); }
            return self.StartCoroutine(ExecuteDelayed(task, delayInSecBeforeExecution));
        }

        private static IEnumerator ExecuteDelayed(Action task, float d1) {
            if (d1 > 0) { yield return new WaitForSeconds(d1); } else { yield return new WaitForEndOfFrame(); }
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
            self.StartCoroutine(WrapRoutine(routine, () => {
                try { tcs.TrySetResult(onRoutineDone()); }
                catch (Exception e) { tcs.TrySetException(e); }
            }));
            return tcs.Task;
        }

        private static IEnumerator WrapRoutine(IEnumerator coroutineToWrap, Action onRoutineDone) {
            yield return coroutineToWrap;
            onRoutineDone();
        }

    }

}
