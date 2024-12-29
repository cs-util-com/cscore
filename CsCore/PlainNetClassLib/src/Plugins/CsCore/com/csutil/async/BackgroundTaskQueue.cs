using com.csutil.progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace com.csutil {

    public class BackgroundTaskQueue : IBackgroundTaskQueue {

        [Obsolete("Use NewBackgroundTaskQueueV2 instead")]
        public static IBackgroundTaskQueue NewBackgroundTaskQueue(int maxConcurrencyLevel) {
            return new BackgroundTaskQueue(new QueuedTaskScheduler(TaskScheduler.Default, maxConcurrencyLevel), new CancellationTokenSource());
        }
        
        public static IBackgroundTaskQueue NewBackgroundTaskQueueV2(int maxConcurrencyLevel) {
            return new BackgroundTaskQueueV2(maxConcurrencyLevel);
        }

        public TaskScheduler Scheduler { get; }
        public CancellationTokenSource Cancel { get; }
        public IProgress ProgressListener { get; set; }

        private ICollection<Task> Tasks { get; } = new List<Task>();

        public BackgroundTaskQueue(QueuedTaskScheduler scheduler, CancellationTokenSource cancelTokenSource) {
            Scheduler = scheduler;
            Cancel = cancelTokenSource;
        }

        public int GetCompletedTasksCount() { return Tasks.Filter(t => t.IsCompleted).Count(); }
        public int GetRemainingScheduledTaskCount() { return Tasks.Filter(t => !t.IsCompleted).Count(); }
        public int GetTotalTasksCount() { return Tasks.Count; }

        public async Task WhenAllTasksCompleted(int millisecondsDelay = 50, bool flushQueueAfterCompletion = false) {
            await Task.WhenAll(Tasks);
            if (millisecondsDelay > 0) { await TaskV2.Delay(millisecondsDelay); }
            // If there were new tasks added in the meantime, do another wait for the remaining tasks:
            if (!IsAllTasksCompleted()) { await WhenAllTasksCompleted(); }
            if (flushQueueAfterCompletion) { Tasks.Clear(); } // Automatically flush task queue after all are done?
        }

        public bool IsAllTasksCompleted() { return GetRemainingScheduledTaskCount() == 0; }

        public void CancelAllOpenTasks() {
            if (!Cancel.IsCancellationRequested) { Cancel.Cancel(); }
        }

        public Task Run(Func<CancellationToken, Task> asyncAction) { return Run(asyncAction, Cancel); }

        public Task Run(Func<CancellationToken, Task> asyncAction, CancellationTokenSource cancellationTokenSource) {
            if (!ReferenceEquals(Cancel, cancellationTokenSource)) {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, Cancel.Token);
            }
            var t = TaskV2.Run(async () => {
                ThrowIfCancellationRequested(cancellationTokenSource);
                await asyncAction(cancellationTokenSource.Token);
                ThrowIfCancellationRequested(cancellationTokenSource);
            }, cancellationTokenSource, Scheduler);
            return AddToManagedTasks(t);
        }

        private static void ThrowIfCancellationRequested(CancellationTokenSource cancel) {
            if (cancel.IsCancellationRequested) { throw new TaskCanceledException(); }
            cancel.Token.ThrowIfCancellationRequested();
        }

        public Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction) { return Run(asyncFunction, Cancel); }

        public async Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationTokenSource cancellationTokenSource) {
            if (!ReferenceEquals(Cancel, cancellationTokenSource)) {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, Cancel.Token);
            }
            Task<T> t = TaskV2.Run(async () => {
                ThrowIfCancellationRequested(cancellationTokenSource);
                T result = await asyncFunction(cancellationTokenSource.Token);
                ThrowIfCancellationRequested(cancellationTokenSource);
                return result;
            }, cancellationTokenSource, Scheduler);
            await AddToManagedTasks(t);
            return await t;
        }

        private async Task AddToManagedTasks(Task taskToAdd) {
            Tasks.Add(taskToAdd);
            ProgressListener?.SetCount(GetCompletedTasksCount(), GetTotalTasksCount());
            try { await taskToAdd; } finally { // After the task is done update the progress listener again:
                ProgressListener?.SetCount(GetCompletedTasksCount(), GetTotalTasksCount());
            }
        }

        public void Dispose() {
            CancelAllOpenTasks();
            Cancel.Dispose();
        }
    }

}