using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil {

    public static class TaskExtensions {

        public static void ThrowIfException(this Task self) {
            if (self.Exception != null) { throw self.Exception; }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> self, int timeoutInMs, bool logReasonIfTimeout = true) {
            var completedTask = await Task.WhenAny(self, TaskV2.Delay(timeoutInMs));
            if (completedTask != self) {
                if (logReasonIfTimeout) { self.LogOnError(); }
                throw new TimeoutException();
            }
            return await self; // use await to propagate exceptions
        }

        public static async Task WithTimeout(this Task self, int timeoutInMs, bool logReasonIfTimeout = true) {
            var completedTask = await Task.WhenAny(self, TaskV2.Delay(timeoutInMs));
            if (completedTask != self) {
                if (logReasonIfTimeout) { self.LogOnError(); }
                throw new TimeoutException();
            }
            await self; // use await to propagate exceptions
        }

        public static async Task OnError(this Task self, Func<Exception, Task> onError) {
            try {
                await self;
            } catch (Exception ex) {
                await onError(ex).ConfigureAwait(false);
            }
        }

        public static async Task<T> OnError<T>(this Task<T> self, Func<Exception, Task<T>> onError) {
            try {
                return await self;
            } catch (Exception ex) {
                // Handle the error and return fallback value:
                return await onError(ex).ConfigureAwait(false);
            }
        }

        public static Task LogOnError(this Task self) {
            return self.LogOnErrorAsync();
        }

        private static async Task LogOnErrorAsync(this Task self) {
            try {
                await self;
            } catch (Exception e) {
                Log.e(e);
                throw;
            }
        }

        /// <summary> Ensures that the continuation action is called on the same syncr. context </summary>
        public static Task ContinueWithSameContext(this Task self, Action<Task> continuationAction) {
            try { // Catch in case current sync context is not allowed to be used as a scheduler (e.g in xUnit)
                return self.ContinueWith(continuationAction, TaskScheduler.FromCurrentSynchronizationContext());
            } catch (Exception) { return self.ContinueWith(continuationAction); }
        }

        /// <summary> Ensures that the continuation action is called on the same syncr. context </summary>
        public static Task ContinueWithSameContext<T>(this Task<T> self, Action<Task<T>> continuationAction) {
            try {
                return self.ContinueWith(continuationAction, TaskScheduler.FromCurrentSynchronizationContext());
            } catch (Exception) { return self.ContinueWith(continuationAction); }
        }

        /// <summary> Returns true if the task is completed but did not fail and was not cancelled. Same as task.IsCompletedSuccessfully </summary>
        public static bool IsCompletedSuccessfull(this Task self) {
            return self.IsCompleted && !(self.IsFaulted || self.IsCanceled);
        }

        public static bool IsNullOrCompleted(this Task self) {
            return self == null || self.IsCompleted;
        }
        
    }

}
