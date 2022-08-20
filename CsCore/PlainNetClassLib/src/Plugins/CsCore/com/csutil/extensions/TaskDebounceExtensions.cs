using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil {

    public static class TaskDebounceExtensions {

        /// <summary>
        /// This will create an async func where the first call is executed and the last call is executed but
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        /// <returns> True if the task did execute and false if it was skipped </returns>
        public static Func<Task<bool>> AsThrottledDebounceV2(this Func<Task> self, double delayInMs, bool skipFirstEvent = false) {
            Func<object, Task> f = (_) => self();
            Func<object, Task<bool>> d = f.AsThrottledDebounceV2(delayInMs, skipFirstEvent);
            return () => d(arg: null);
        }

        /// <summary>
        /// This will create an async func where the first call is executed and the last call is executed but
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        /// <returns> True if the task did execute and false if it was skipped </returns>
        public static Func<T, Task<bool>> AsThrottledDebounceV2<T>(this Func<T, Task> self, double delayInMs, bool skipFirstEvent = false) {
            Func<T, Task<bool>> f = async (t) => {
                await self(t);
                return true;
            };
            Func<T, Task<bool>> d = f.AsThrottledDebounceV2(delayInMs, skipFirstEvent);
            return async (t) => {
                try { return await d(t); } catch (TaskSkippedException<bool>) { return false; }
            };
        }

        /// <summary>
        /// This will create an async func where the first call is executed and the last call is executed but
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        /// <exception cref="TaskSkippedException{T}"> If the func was canceled because another one after it replaced it the returned Task will indicate this </exception>
        public static Func<Task<T>> AsThrottledDebounceV2<T>(this Func<Task<T>> self, double delayInMs, bool skipFirstEvent = false) {
            Func<object, Task<T>> f = (_) => self();
            Func<object, Task<T>> d = f.AsThrottledDebounceV2(delayInMs, skipFirstEvent);
            return () => d(arg: null);
        }


        /// <summary>
        /// This will create an async func where the first call is executed and the last call is executed but
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        /// <exception cref="TaskSkippedException{T}"> If the func was canceled because another one after it replaced it the returned Task will indicate this </exception>
        public static Func<T, Task<V>> AsThrottledDebounceV2<T, V>(this Func<T, Task<V>> self, double delayInMs, bool skipFirstEvent = false) {
            int triggerFirstEvent = skipFirstEvent ? 0 : 1;
            int last = 0;
            Task<V> latestRunTask = null;
            return async (t) => {
                var current = Interlocked.Increment(ref last);
                if (!skipFirstEvent && ThreadSafety.FlipToFalse(ref triggerFirstEvent)) {
                    latestRunTask = self(t);
                    return await latestRunTask;
                }
                await TaskV2.Delay((int)delayInMs);
                if (current != last) { throw new TaskSkippedException<V>(latestRunTask); }
                // Wait for the previous execution to finish in case it didnt yet:
                try { await latestRunTask; } catch (Exception) { }
                latestRunTask = self(t);
                return await latestRunTask;
            };
        }

        /// <summary>
        /// This will create an async func where the first call is executed and the last call is executed but
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        /// <exception cref="TaskCanceledException"> If the func was canceled because another one after it replaced it the returned Task will indicate this </exception>
        [Obsolete("Use AsThrottledDebounceV2 instead")]
        public static Func<Task> AsThrottledDebounce(this Func<Task> self, double delayInMs, bool skipFirstEvent = false) {
            Func<object, Task> f = (_) => self();
            Func<object, Task> d = f.AsThrottledDebounce(delayInMs, skipFirstEvent);
            return () => d(arg: null);
        }

        /// <summary>
        /// This will create an async func where the first call is executed and the last call is executed but
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        /// <exception cref="TaskCanceledException"> If the func was canceled because another one after it replaced it the returned Task will indicate this </exception>
        [Obsolete("Use AsThrottledDebounceV2 instead")]
        public static Func<T, Task> AsThrottledDebounce<T>(this Func<T, Task> self, double delayInMs, bool skipFirstEvent = false) {
            int triggerFirstEvent = skipFirstEvent ? 0 : 1;
            int last = 0;
            return async (t) => {
                var current = Interlocked.Increment(ref last);
                if (!skipFirstEvent && ThreadSafety.FlipToFalse(ref triggerFirstEvent)) {
                    await self(t);
                } else {
                    await TaskV2.Delay((int)delayInMs);
                    if (current == last) {
                        await self(t);
                    } else {
                        throw new TaskCanceledException();
                    }
                }
            };
        }

    }

    [Serializable]
    public class TaskSkippedException<T> : TaskCanceledException {
        public readonly Task<T> LatestRunTask;
        public TaskSkippedException() { }
        public TaskSkippedException(string message) : base(message) { }
        public TaskSkippedException(string message, Exception inner) : base(message, inner) { }
        protected TaskSkippedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public TaskSkippedException(Task<T> latestRunTask) { this.LatestRunTask = latestRunTask; }
    }

}