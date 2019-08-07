using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace com.csutil.async {

    public class TaskHelper {

        public static async Task TryWithExponentialBackoff(Func<Task> taskToTry,
                        Action<Exception> onError = null, int maxNrOfRetries = -1, int maxDelayInMs = -1, int initialExponent = 1) {

            await TryWithExponentialBackoff<bool>(async () => {
                await taskToTry();
                return true;
            }, onError, maxNrOfRetries, maxDelayInMs, initialExponent);

        }

        public static async Task<T> TryWithExponentialBackoff<T>(Func<Task<T>> taskToTry,
                        Action<Exception> onError = null, int maxNrOfRetries = -1, int maxDelayInMs = -1, int initialExponent = 1) {

            int retryCount = initialExponent;
            Stopwatch timer = Stopwatch.StartNew();
            do {
                timer.Restart();
                try {
                    Task<T> task = taskToTry();
                    var result = await task;
                    if (!task.IsFaulted && !task.IsFaulted) { return result; }
                } catch (Exception e) { onError.InvokeIfNotNull(e); }
                retryCount++;
                int delay = (int)(Math.Pow(2, retryCount) - timer.ElapsedMilliseconds);
                if (delay > maxDelayInMs && maxDelayInMs > 0) { delay = maxDelayInMs; }
                if (delay > 0) { await Task.Delay(delay); }
                if (retryCount > maxNrOfRetries && maxNrOfRetries > 0) {
                    throw new OperationCanceledException("No success after " + retryCount + " retries");
                }
            } while (true);

        }

    }

}