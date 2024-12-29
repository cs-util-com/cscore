using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.progress;

namespace com.csutil {

    public class BackgroundTaskQueueV2 : IBackgroundTaskQueue, IDisposable {

        /// <summary>
        /// Interface for a queued work item that can run or be canceled.
        /// </summary>
        private interface IWorkItem {
            /// <summary> The underlying task (or TCS.Task) for this work item. </summary>
            Task BaseTask { get; }

            /// <summary>
            /// Executes the work item with the given CancellationToken.
            /// </summary>
            Task Run(CancellationToken token);

            /// <summary>
            /// Marks this work item as canceled, so that BaseTask transitions to Canceled state.
            /// </summary>
            void Cancel();
        }

        /// <summary>
        /// Non-generic WorkItem (for Task with no result).
        /// </summary>
        private class WorkItem : IWorkItem {
            private readonly Func<CancellationToken, Task> _action;
            private readonly TaskCompletionSource<object> _tcs;

            public WorkItem(Func<CancellationToken, Task> action, TaskCompletionSource<object> tcs) {
                _action = action;
                _tcs = tcs;
            }

            public Task BaseTask => _tcs.Task;

            public async Task Run(CancellationToken token) {
                try {
                    token.ThrowIfCancellationRequested();
                    await _action(token);
                    _tcs.SetResult(null);
                } catch (OperationCanceledException) {
                    _tcs.SetCanceled();
                } catch (Exception ex) {
                    _tcs.SetException(ex);
                }
            }

            public void Cancel() {
                _tcs.TrySetCanceled();
            }
        }

        /// <summary>
        /// Generic WorkItem (for Task&lt;T&gt;).
        /// </summary>
        private class WorkItem<T> : IWorkItem {
            private readonly Func<CancellationToken, Task<T>> _func;
            private readonly TaskCompletionSource<T> _tcs;

            public WorkItem(Func<CancellationToken, Task<T>> func, TaskCompletionSource<T> tcs) {
                _func = func;
                _tcs = tcs;
            }

            public Task BaseTask => _tcs.Task;

            public async Task Run(CancellationToken token) {
                try {
                    token.ThrowIfCancellationRequested();
                    var result = await _func(token);
                    _tcs.SetResult(result);
                } catch (OperationCanceledException) {
                    _tcs.SetCanceled();
                } catch (Exception ex) {
                    _tcs.SetException(ex);
                }
            }

            public void Cancel() {
                _tcs.TrySetCanceled();
            }
        }

        // --------------------------------------------------------------------

        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _globalCts;

        // We store IWorkItem objects instead of just Func<...>.
        private readonly Queue<IWorkItem> _workQueue = new Queue<IWorkItem>();
        private bool _dispatcherRunning;

        // Track all tasks (including canceled) so that WhenAllTasksCompleted() can see them.
        private readonly List<Task> _trackedTasks = new List<Task>();

        public IProgress ProgressListener { get; set; }

        public BackgroundTaskQueueV2(int maxConcurrency, CancellationTokenSource cancellationTokenSource = null) {
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            _globalCts = cancellationTokenSource ?? new CancellationTokenSource();
        }

        public Task Run(Func<CancellationToken, Task> asyncAction) {
            return Run(asyncAction, _globalCts);
        }

        public Task Run(Func<CancellationToken, Task> asyncAction, CancellationTokenSource cts) {
            if (!ReferenceEquals(_globalCts, cts)) {
                cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _globalCts.Token);
            }

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var workItem = new WorkItem(asyncAction, tcs);

            lock (_workQueue) {
                _workQueue.Enqueue(workItem);
            }

            _ = RunDispatcher(cts.Token);
            _ = AddToManagedTasks(workItem.BaseTask);
            return workItem.BaseTask;
        }

        public Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction) {
            return Run(asyncFunction, _globalCts);
        }

        public Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationTokenSource cts) {
            if (!ReferenceEquals(_globalCts, cts)) {
                cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _globalCts.Token);
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            var workItem = new WorkItem<T>(asyncFunction, tcs);

            lock (_workQueue) {
                _workQueue.Enqueue(workItem);
            }

            _ = RunDispatcher(cts.Token);
            _ = AddToManagedTasks(workItem.BaseTask);
            return (Task<T>)workItem.BaseTask;
        }

        private async Task RunDispatcher(CancellationToken token) {
            lock (_workQueue) {
                if (_dispatcherRunning) return;
                _dispatcherRunning = true;
            }

            try {
                while (true) {
                    IWorkItem nextWorkItem = null;
                    lock (_workQueue) {
                        if (_workQueue.Count > 0) {
                            nextWorkItem = _workQueue.Dequeue();
                        } else {
                            _dispatcherRunning = false;
                            return;
                        }
                    }

                    try {
                        // No change here
                        await _semaphore.WaitAsync(token);
                    } catch (OperationCanceledException) {
                        // Cancel the dequeued work item that triggered this exception:
                        nextWorkItem.Cancel();

                        // Also immediately cancel all remaining tasks in the queue,
                        // because we won't be processing them anymore:
                        lock (_workQueue) {
                            while (_workQueue.Count > 0) {
                                _workQueue.Dequeue().Cancel();
                            }
                        }

                        // Finally, stop this dispatcher thread:
                        _dispatcherRunning = false;
                        return;
                    }

                    _ = Task.Run(async () => {
                        try {
                            token.ThrowIfCancellationRequested();
                            await nextWorkItem.Run(token);
                        } finally {
                            _semaphore.Release();
                        }
                    }, token);
                }
            } catch {
                lock (_workQueue) {
                    _dispatcherRunning = false;
                }
                throw;
            }
        }

        private async Task AddToManagedTasks(Task task) {
            lock (_trackedTasks) {
                _trackedTasks.Add(task);
            }
            // Provide initial progress
            ProgressListener?.SetCount(GetCompletedTasksCount(), GetTotalTasksCount());

            try {
                await task;
            } finally {
                // Provide updated progress
                ProgressListener?.SetCount(GetCompletedTasksCount(), GetTotalTasksCount());
            }
        }

        public int GetRemainingScheduledTaskCount() {
            lock (_trackedTasks) {
                return _trackedTasks.Count(t => !t.IsCompleted);
            }
        }

        public int GetCompletedTasksCount() {
            lock (_trackedTasks) {
                return _trackedTasks.Count(t => t.IsCompleted);
            }
        }

        public int GetTotalTasksCount() {
            lock (_trackedTasks) {
                return _trackedTasks.Count;
            }
        }

        public async Task WhenAllTasksCompleted(int millisecondsDelay = 50, bool flushQueueAfterCompletion = false) {
            List<Task> currentSnapshot;
            lock (_trackedTasks) {
                currentSnapshot = _trackedTasks.ToList();
            }
            await Task.WhenAll(currentSnapshot);

            if (millisecondsDelay > 0) {
                await Task.Delay(millisecondsDelay);
            }

            if (!IsAllTasksCompleted()) {
                await WhenAllTasksCompleted(millisecondsDelay, flushQueueAfterCompletion);
            } else if (flushQueueAfterCompletion) {
                lock (_trackedTasks) {
                    _trackedTasks.Clear();
                }
            }
        }

        private bool IsAllTasksCompleted() => GetRemainingScheduledTaskCount() == 0;

        public void CancelAllOpenTasks() {
            // Cancel the global token so that in-flight tasks see the cancellation:
            if (!_globalCts.IsCancellationRequested) {
                _globalCts.Cancel();
            }

            // Immediately cancel all tasks still in the queue (that haven't started).
            lock (_workQueue) {
                while (_workQueue.Count > 0) {
                    var leftoverWork = _workQueue.Dequeue();
                    leftoverWork.Cancel();
                }
            }
        }

        public void Dispose() {
            CancelAllOpenTasks();
            _globalCts.Dispose();
            _semaphore.Dispose();
        }

    }
}