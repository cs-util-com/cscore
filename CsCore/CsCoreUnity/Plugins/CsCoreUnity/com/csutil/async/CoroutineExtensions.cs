using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class CoroutineExtensions {

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
