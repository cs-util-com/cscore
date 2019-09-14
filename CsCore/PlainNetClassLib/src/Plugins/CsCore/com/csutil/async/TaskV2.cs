using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public class TaskV2 {

        private static int lastOverhead = 0;

        public static async Task Delay(int millisecondsDelay) {
            millisecondsDelay = Math.Max(millisecondsDelay - lastOverhead, 1);
            var t = Stopwatch.StartNew();
            await IoC.inject.GetOrAddSingleton<TaskV2>(null).DelayTask(millisecondsDelay);
            t.Stop();
            lastOverhead = (int)(t.ElapsedMilliseconds - millisecondsDelay);
            if (lastOverhead < 0) { // The wait was shorter then requested:
                await Delay(-lastOverhead); // wait the additional difference
            }
        }

        public static Task Delay(TimeSpan t) { return Delay((int)t.TotalMilliseconds); }

        protected virtual Task DelayTask(int millisecondsDelay) { return Task.Delay(millisecondsDelay); }

        public static Task Run(Action action) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(action);
        }

        protected virtual Task RunTask(Action action) { return Task.Run(action); }

        public static Task Run(Func<Task> asyncAction) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncAction);
        }

        protected virtual Task RunTask(Func<Task> asyncAction) { return Task.Run(asyncAction); }

        public static Task<T> Run<T>(Func<Task<T>> asyncAction) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncAction);
        }

        protected virtual Task<T> RunTask<T>(Func<Task<T>> asyncAction) { return Task.Run(asyncAction); }

        public static async Task TryWithExponentialBackoff(Func<Task> taskToTry,
                        Action<Exception> onError = null, int maxNrOfRetries = -1, int maxDelayInMs = -1, int initialExponent = 0) {

            await TryWithExponentialBackoff<bool>(async () => {
                await taskToTry();
                return true;
            }, onError, maxNrOfRetries, maxDelayInMs, initialExponent);

        }

        public static async Task<T> TryWithExponentialBackoff<T>(Func<Task<T>> taskToTry,
                        Action<Exception> onError = null, int maxNrOfRetries = -1, int maxDelayInMs = -1, int initialExponent = 0) {

            int retryCount = initialExponent;
            Stopwatch timer = Stopwatch.StartNew();
            do {
                timer.Restart();
                try {
                    Task<T> task = taskToTry();
                    var result = await task;
                    if (!task.IsFaulted && !task.IsFaulted) { return result; }
                }
                catch (Exception e) { onError.InvokeIfNotNull(e); }
                retryCount++;
                int delay = (int)(Math.Pow(2, retryCount) - timer.ElapsedMilliseconds);
                if (delay > maxDelayInMs && maxDelayInMs > 0) { delay = maxDelayInMs; }
                if (delay > 0) { await TaskV2.Delay(delay); }
                if (retryCount >= maxNrOfRetries && maxNrOfRetries > 0) {
                    throw new OperationCanceledException("No success after " + retryCount + " retries");
                }
            } while (true);

        }

    }

}
