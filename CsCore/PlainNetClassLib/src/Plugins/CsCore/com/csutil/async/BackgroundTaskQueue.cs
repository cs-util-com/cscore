using com.csutil.progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace com.csutil {

    public interface IBackgroundTaskQueue {
        TaskScheduler Scheduler { get; }
        CancellationTokenSource Cancel { get; }
        ICollection<Task> Tasks { get; }
        IProgress ProgressListener { set; get; }
    }

    public static class TaskQueueExtensions {

        public static int GetCompletedTasksCount(this IBackgroundTaskQueue self) { return self.Tasks.Filter(t => t.IsCompleted).Count(); }
        public static int GetRemainingScheduledTaskCount(this IBackgroundTaskQueue self) { return self.Tasks.Filter(t => !t.IsCompleted).Count(); }
        public static int GetTotalTasksCount(this IBackgroundTaskQueue self) { return self.Tasks.Count; }

        public static async Task WhenAllTasksCompleted(this IBackgroundTaskQueue self, int millisecondsDelay = 50, bool flushQueueAfterCompletion = false) {
            await Task.WhenAll(self.Tasks);
            if (millisecondsDelay > 0) { await TaskV2.Delay(millisecondsDelay); }
            // If there were new tasks added in the meantime, do another wait for the remaining tasks:
            if (!self.IsAllTasksCompleted()) { await self.WhenAllTasksCompleted(); }
            self.Tasks.Clear(); // Automatically flush task queue after all are done?
        }

        public static bool IsAllTasksCompleted(this IBackgroundTaskQueue self) { return self.GetRemainingScheduledTaskCount() == 0; }

        public static void CancelAllOpenTasks(this IBackgroundTaskQueue self) {
            if (!self.Cancel.IsCancellationRequested) { self.Cancel.Cancel(); }
        }

        public static async Task Run(this IBackgroundTaskQueue self, Func<Task> asyncAction) {
            var t = TaskV2.Run(asyncAction, self.Cancel, self.Scheduler);
            await AddToManagedTasks(self, t);
        }

        public static async Task<T> Run<T>(this IBackgroundTaskQueue self, Func<Task<T>> asyncFunction) {
            var t = TaskV2.Run(asyncFunction, self.Cancel, self.Scheduler);
            await AddToManagedTasks(self, t);
            return await t;
        }

        private static async Task AddToManagedTasks(IBackgroundTaskQueue self, Task taskToAdd) {
            self.Tasks.Add(taskToAdd);
            self.ProgressListener?.SetCount(self.GetCompletedTasksCount(), self.GetTotalTasksCount());
            await taskToAdd; // After the task is done update the progress listener again:
            self.ProgressListener?.SetCount(self.GetCompletedTasksCount(), self.GetTotalTasksCount());
        }

    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue {

        public static IBackgroundTaskQueue NewBackgroundTaskQueue(int maxConcurrencyLevel) {
            return new BackgroundTaskQueue(new QueuedTaskScheduler(TaskScheduler.Default, maxConcurrencyLevel), new CancellationTokenSource());
        }

        public TaskScheduler Scheduler { get; }
        public CancellationTokenSource Cancel { get; }
        ICollection<Task> IBackgroundTaskQueue.Tasks { get; } = new List<Task>();
        IProgress IBackgroundTaskQueue.ProgressListener { get; set; }

        public BackgroundTaskQueue(QueuedTaskScheduler scheduler, CancellationTokenSource cancelTokenSource) {
            Scheduler = scheduler;
            Cancel = cancelTokenSource;
        }

    }

}