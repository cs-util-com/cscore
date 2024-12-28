using com.csutil.progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil {

    /// <summary>
    /// Improved task queue that uses SemaphoreSlim to control concurrency.
    /// </summary>
    public class BackgroundTaskQueueV2 : IBackgroundTaskQueue {
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _globalCts;
        private readonly List<Task> _tasks = new List<Task>();

        public IProgress ProgressListener { get; set; }

        /// <summary>
        /// Creates a new background task queue using the default thread pool
        /// and the provided maximum concurrency level.
        /// </summary>
        /// <param name="maxConcurrency">Max number of tasks to run in parallel.</param>
        /// <param name="cancellationTokenSource">
        /// Optional global cancellation token source. If null, a new one is created.
        /// </param>
        public BackgroundTaskQueueV2(int maxConcurrency, CancellationTokenSource cancellationTokenSource = null) {
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            _globalCts = cancellationTokenSource ?? new CancellationTokenSource();
        }

        public Task Run(Func<CancellationToken, Task> asyncAction) {
            return Run(asyncAction, _globalCts);
        }

        public Task Run(Func<CancellationToken, Task> asyncAction, CancellationTokenSource cancellationTokenSource) {
            // If a different CTS is given, link it to the global CTS
            if (!ReferenceEquals(_globalCts, cancellationTokenSource)) {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenSource.Token, _globalCts.Token);
            }

            var task = TaskV2.Run(async () => {
                // Wait for an available slot
                await _semaphore.WaitAsync(cancellationTokenSource.Token);
                try {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await asyncAction(cancellationTokenSource.Token);
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                } finally {
                    _semaphore.Release();
                }
            });

            return AddToManagedTasks(task);
        }

        public Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction) {
            return Run(asyncFunction, _globalCts);
        }

        public async Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationTokenSource cancellationTokenSource) {
            // If a different CTS is given, link it to the global CTS
            if (!ReferenceEquals(_globalCts, cancellationTokenSource)) {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenSource.Token, _globalCts.Token);
            }

            var task = TaskV2.Run(async () => {
                await _semaphore.WaitAsync(cancellationTokenSource.Token);
                try {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    var result = await asyncFunction(cancellationTokenSource.Token);
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    return result;
                } finally {
                    _semaphore.Release();
                }
            });

            // Add the task to the list for progress and completion tracking
            await AddToManagedTasks(task);
            return await task;
        }

        /// <summary>
        /// Keeps track of the task for overall completion, updates progress, etc.
        /// </summary>
        private async Task AddToManagedTasks(Task task) {
            lock (_tasks) {
                _tasks.Add(task);
            }

            // Update progress before the task starts
            ProgressListener?.SetCount(GetCompletedTasksCount(), GetTotalTasksCount());

            try {
                // Wait for the task to complete
                await task;
            } finally {
                // Update progress once the task has finished
                ProgressListener?.SetCount(GetCompletedTasksCount(), GetTotalTasksCount());
            }
        }

        public int GetRemainingScheduledTaskCount() {
            lock (_tasks) {
                return _tasks.Count(t => !t.IsCompleted);
            }
        }

        public int GetCompletedTasksCount() {
            lock (_tasks) {
                return _tasks.Count(t => t.IsCompleted);
            }
        }

        public int GetTotalTasksCount() {
            lock (_tasks) {
                return _tasks.Count;
            }
        }

        public async Task WhenAllTasksCompleted(int millisecondsDelay = 50, bool flushQueueAfterCompletion = false) {
            List<Task> currentSnapshot;
            lock (_tasks) {
                currentSnapshot = _tasks.ToList();
            }

            // Wait for all tasks in the current snapshot to complete
            await Task.WhenAll(currentSnapshot);

            // Optional small delay to allow for any newly spawned tasks to get queued
            if (millisecondsDelay > 0) {
                await Task.Delay(millisecondsDelay);
            }

            // Check if new tasks arrived after the initial snapshot
            if (!IsAllTasksCompleted()) {
                // Recursively wait again until everything is done
                await WhenAllTasksCompleted(millisecondsDelay, flushQueueAfterCompletion);
            } else if (flushQueueAfterCompletion) {
                lock (_tasks) {
                    _tasks.Clear();
                }
            }
        }

        private bool IsAllTasksCompleted() => GetRemainingScheduledTaskCount() == 0;

        public void CancelAllOpenTasks() {
            if (!_globalCts.IsCancellationRequested) {
                _globalCts.Cancel();
            }
        }

        public void Dispose() {
            CancelAllOpenTasks();
            _globalCts.Dispose();
            _semaphore.Dispose();
        }
    }
}