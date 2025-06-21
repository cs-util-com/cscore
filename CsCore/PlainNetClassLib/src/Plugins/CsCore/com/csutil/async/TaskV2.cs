﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil {

    public class TaskV2 {

        private static int lastOverhead = 0;

        public static Task Delay(int millisecondsDelay) { return Delay(millisecondsDelay, CancellationToken.None); }

        public static async Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
            millisecondsDelay = Math.Max(millisecondsDelay - lastOverhead, 1);
            var t = Stopwatch.StartNew();
            await IoC.inject.GetOrAddSingleton<TaskV2>(null).DelayTask(millisecondsDelay, cancellationToken);
            t.Stop();
            lastOverhead = (int)(t.ElapsedMilliseconds - millisecondsDelay);
            if (lastOverhead < 0) { // The wait was shorter then requested:
                await Delay(-lastOverhead, cancellationToken); // wait the additional difference
            }
        }

        public static Task Delay(TimeSpan t) { return Delay((int)t.TotalMilliseconds); }

        protected virtual Task DelayTask(int millisecondsDelay, CancellationToken cancellationToken) { return Task.Delay(millisecondsDelay, cancellationToken); }

        public static Task Run(Action action) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(action);
        }

        protected virtual Task RunTask(Action action) { return Task.Run(action); }

        public static Task Run(Func<Task> asyncAction) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncAction);
        }

        protected virtual Task RunTask(Func<Task> asyncAction) {
#if UNITY_EDITOR
            return Task.Run(() => {
                using var t = Log.BeginThreadProfiling();
                t.ThrowErrorIfNull("StopwatchV2");
                return asyncAction();
            });
#endif
            return Task.Run(asyncAction);
        }

        public static Task Run(Func<Task> asyncAction, CancellationTokenSource cancel, TaskScheduler scheduler) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncAction, cancel, scheduler);
        }

        [Obsolete("Consider using BackgroundTaskQueue.Run instead")]
        protected virtual Task RunTask(Func<Task> asyncAction, CancellationTokenSource cancel, TaskScheduler scheduler) {
            return Task.Factory.StartNew(() => Wait(Run(asyncAction)), cancel.Token, TaskCreationOptions.None, scheduler);
        }

        public static Task<T> Run<T>(Func<Task<T>> asyncFunction) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncFunction);
        }

        protected virtual Task<T> RunTask<T>(Func<Task<T>> asyncFunction) { return Task.Run(asyncFunction); }

        [Obsolete("Consider using BackgroundTaskQueue.Run instead")]
        public static Task<T> Run<T>(Func<Task<T>> asyncFunction, CancellationTokenSource cancel, TaskScheduler scheduler) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncFunction, cancel, scheduler);
        }

        protected virtual Task<T> RunTask<T>(Func<Task<T>> asyncFunction, CancellationTokenSource cancel, TaskScheduler scheduler) {
            return Task.Factory.StartNew(() => Wait(Run(asyncFunction)), cancel.Token, TaskCreationOptions.None, scheduler).Unwrap();
        }

        private static T Wait<T>(T task) where T : Task { task.Wait(); return task; }

        public static async Task TryWithExponentialBackoff(Func<Task> taskToTry,
                        Action<Exception> onError = null, int maxNrOfRetries = -1, int maxDelayInMs = -1, int initialExponent = 0) {

            await TryWithExponentialBackoff<bool>(async () => {
                await taskToTry();
                return true;
            }, onError, maxNrOfRetries, maxDelayInMs, initialExponent);

        }

        /// <param name="initialExponent">e.g 9 => 2^9 = 512ms (for first delay if fails) </param>
        public static async Task<T> TryWithExponentialBackoff<T>(Func<Task<T>> taskToTry,
                        Action<Exception> onError = null, int maxNrOfRetries = -1, int maxDelayInMs = -1, int initialExponent = 0) {

            int maxExponent = maxNrOfRetries + initialExponent;
            int currentExponent = initialExponent;
            Stopwatch timer = Stopwatch.StartNew();
            Exception latestError = null;
            do {
                timer.Restart();
                try {
                    Task<T> task = taskToTry();
                    var result = await task;
                    if (!task.IsFaulted && !task.IsFaulted) { return result; }
                } catch (Exception e) {
                    onError.InvokeIfNotNull(e);
                    latestError = e;
                }
                currentExponent++;
                int delay = (int)(Math.Pow(2, currentExponent) - timer.ElapsedMilliseconds);
                if (delay > maxDelayInMs && maxDelayInMs > 0) { delay = maxDelayInMs; }
                if (delay > 0) { await TaskV2.Delay(delay); }
                if (maxNrOfRetries > 0 && currentExponent >= maxExponent) {
                    throw new OperationCanceledException($"No success after {maxNrOfRetries} retries", latestError);
                }
            } while (true);

        }

        public static async Task RunRepeated(Func<Task<bool>> task, int delayInMsBetweenIterations, CancellationToken cancelToken, int delayInMsBeforeFirstExecution = 0, float repetitions = -1) {
            if (delayInMsBeforeFirstExecution > 0) { await Delay(delayInMsBeforeFirstExecution); }
            while (repetitions != 0) {
                cancelToken.ThrowIfCancellationRequested();
                if (!await task()) { break; }
                await TaskV2.Delay(delayInMsBetweenIterations);
                repetitions--;
            }
        }

        public static Task<Task> WhenAnySuccessful(params Task[] tasks) {
            return WhenAnySuccessful(true, tasks);
        }

        public static Task<Task> WhenAnySuccessful(bool logErrorsForUnsuccessfuls, params Task[] tasks) {
            if (logErrorsForUnsuccessfuls) { Task.WhenAll(tasks).LogOnError(); }
            return WhenAnySuccessfulInner(tasks);
        }

        public static Task<Task> WhenAnySuccessful(IEnumerable<Task> tasks, bool logErrorsForUnsuccessfuls = true) {
            var t = tasks.Cached();
            if (logErrorsForUnsuccessfuls) { Task.WhenAll(t).LogOnError(); }
            return WhenAnySuccessfulInner(t);
        }

        private static async Task<Task> WhenAnySuccessfulInner(IEnumerable<Task> tasks) {
            var completedTask = await Task.WhenAny(tasks);
            if (!completedTask.IsCompletedSuccessfull()) {
                var allExceptFailed = new List<Task>();
                foreach (var task in tasks) {
                    // If any other successful task is found, directly return that one:
                    if (task.IsCompletedSuccessfull()) { return task; }
                    // Filter out the failed completedTask
                    if (task != completedTask) { allExceptFailed.Add(task); }
                }
                if (allExceptFailed.IsNullOrEmpty()) {
                    await completedTask; // Await the last failed one in the list to trigger an exception
                }
                return await WhenAnySuccessfulInner(allExceptFailed);
            }
            return completedTask;
        }

    }

}
