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

        public static async Task<T> WithTimeout<T>(this Task<T> self, int timeoutInMs) {
            var completedTask = await Task.WhenAny(self, TaskV2.Delay(timeoutInMs));
            if (completedTask != self) { throw new TimeoutException(); }
            return await self;  // use await to propagate exceptions
        }

        public static async Task WithTimeout(this Task self, int timeoutInMs) {
            var completedTask = await Task.WhenAny(self, TaskV2.Delay(timeoutInMs));
            if (completedTask != self) { throw new TimeoutException(); }
            await self;  // use await to propagate exceptions
        }

        public static Task OnError(this Task self, Func<Exception, Task> onError) {
            return self.ContinueWith(t => { if (t.IsFaulted) { return onError(t.Exception); } return t; }).Unwrap();
        }

        public static Task<T> OnError<T>(this Task<T> self, Func<Exception, Task<T>> onError) {
            return self.ContinueWith(t => { if (t.IsFaulted) { return onError(t.Exception); } return t; }).Unwrap();
        }

        public static Task LogOnError(this Task self) {
            return self.OnError(e => Task.FromException(Log.e(e)));
        }

        public static void ForwardResultTo<T>(this Task<T> self, TaskCompletionSource<T> tcs) {
            self.ContinueWith(task => {
                if (!task.IsCanceledOrFaulted(tcs)) { tcs.TrySetResult(task.Result); }
            });
        }

        public static void ForwardResultTo<T>(this Task self, TaskCompletionSource<T> tcs) {
            self.ContinueWith(task => {
                if (!task.IsCanceledOrFaulted(tcs)) { tcs.TrySetResult((T)(object)task); }
            });
        }

        private static bool IsCanceledOrFaulted<T>(this Task task, TaskCompletionSource<T> tcs) {
            if (task.IsCanceled) {
                tcs.TrySetCanceled();
                return true;
            } else if (task.IsFaulted) {
                tcs.TrySetException(task.Exception);
                return true;
            }
            return false;
        }
    }

}
