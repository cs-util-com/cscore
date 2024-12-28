using System;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.progress;

namespace com.csutil {
    
    public interface IBackgroundTaskQueue : IDisposable {

        Task Run(Func<CancellationToken, Task> asyncAction);
        Task Run(Func<CancellationToken, Task> asyncAction, CancellationTokenSource cancellationTokenSource);

        Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction);
        Task<T> Run<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationTokenSource cancellationTokenSource);
        int GetRemainingScheduledTaskCount();
        int GetCompletedTasksCount();
        int GetTotalTasksCount();
        IProgress ProgressListener { get; set; }
        Task WhenAllTasksCompleted(int millisecondsDelay = 50, bool flushQueueAfterCompletion = false);
        void CancelAllOpenTasks();
    }
    
}