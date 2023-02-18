using System;
using System.Threading.Tasks;

namespace com.csutil {

    public static class TaskCompletionSourceExtensions {

        public static void SetFromTask<T>(this TaskCompletionSource<T> self, Task<T> task) {
            var t = RunAwaitableTscConnectorMethod(self, task);
        }

        public static void SetFromTask(this TaskCompletionSource<bool> self, Task task) {
            var t = RunAwaitableTscConnectorMethod(self, WrapWithBoolTask(task));
        }

        private static async Task<bool> WrapWithBoolTask(Task task) {
            await task;
            return true;
        }

        /// <summary> Idea from https://github.com/dotnet/runtime/issues/47998#issue-803716222  </summary>
        private static async Task RunAwaitableTscConnectorMethod<T>(TaskCompletionSource<T> self, Task<T> task) {
            try {
                self.TrySetResult(await task);
            } catch (OperationCanceledException ex) {
                self.TrySetCanceled(ex.CancellationToken);
            } catch (Exception ex) {
                self.TrySetException(ex);
            }
        }

    }

}