using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class CoroutineExtensions {

        public static Coroutine ExecuteRepeated(this MonoBehaviour self, Func<bool> task,
                float delayInSecBetweenIterations, float delayInSecBeforeFirstExecution = 0, float repetitions = -1) {
            return self.StartCoroutine(ExecuteRepeated(task,
                delayInSecBetweenIterations, delayInSecBeforeFirstExecution, repetitions));
        }

        private static IEnumerator ExecuteRepeated(Func<bool> task, float repeatDelay, float firstDelay = 0, float rep = -1) {
            if (firstDelay > 0) { yield return new WaitForSeconds(firstDelay); }
            var waitTask = new WaitForSeconds(repeatDelay);
            while (rep != 0 && task()) { rep--; yield return waitTask; }
        }

        public static Coroutine ExecuteDelayed(this MonoBehaviour self, Action task, float delayInSecBeforeExecution = 0f) {
            return self.StartCoroutine(ExecuteDelayed(task, delayInSecBeforeExecution));
        }

        private static IEnumerator ExecuteDelayed(Action task, float d1) {
            if (d1 > 0) { yield return new WaitForSeconds(d1); } else { yield return new WaitForEndOfFrame(); }
            task();
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

    }

}
