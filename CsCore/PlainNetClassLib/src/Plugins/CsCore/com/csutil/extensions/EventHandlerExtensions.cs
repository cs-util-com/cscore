using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {


    public static class EventHandlerExtensions {

        public class EventHandlerResult<T> { public T result; }

        public static Func<T, R> AsThrottledDebounce<T, R>(this Func<T, R> self, double delayInMs, bool skipFirstEvent = false) {
            EventHandler<EventHandlerResult<R>> action = (input, output) => { output.result = self((T)input); };
            EventHandler<EventHandlerResult<R>> throttledAction = action.AsThrottledDebounce(delayInMs, skipFirstEvent);
            return (T input) => {
                var output = new EventHandlerResult<R>();
                throttledAction(input, output);
                return output.result;
            };
        }

        /// <summary> 
        /// This will create an EventHandler where the first call is executed and the last call is executed but 
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        public static EventHandler<T> AsThrottledDebounce<T>(this EventHandler<T> self, double delayInMs, bool skipFirstEvent = false) {
            object threadLock = new object();
            T latestEventArgs;
            Stopwatch s = Stopwatch.StartNew();
            bool triggerFirstEvent = !skipFirstEvent;
            Func<object, T, Task> asyncEventHandler = async (sender, eventArgs) => {
                lock (threadLock) {
                    latestEventArgs = eventArgs;
                    s.Restart();
                    if (triggerFirstEvent) {
                        triggerFirstEvent = false;
                        self(sender, eventArgs);
                    }
                }
                var delay = TaskV2.Delay((int)(delayInMs * 1.1f));
                await HandlerTaskResultIfNeeded(eventArgs);
                await delay;
                if (s.ElapsedMilliseconds >= delayInMs) {
                    lock (threadLock) {
                        if (s.ElapsedMilliseconds >= delayInMs) {
                            s.Reset(); // Stop (and reset) and only continue below
                            self(sender, latestEventArgs);
                            if (!skipFirstEvent) { triggerFirstEvent = true; }
                        }
                    }
                    await HandlerTaskResultIfNeeded(latestEventArgs);
                    s.Restart();
                }
            };
            return (sender, eventArgs) => {
                asyncEventHandler(sender, eventArgs).ContinueWithSameContext(finishedTask => {
                    // A failed task cant be awaited but it can be logged
                    if (finishedTask.Exception != null) { Log.e(finishedTask.Exception); }
                });
            };
        }

        private static async Task HandlerTaskResultIfNeeded(object eventArgs) {
            if (eventArgs is EventHandlerResult<Task> t && t.result != null) { await t.result; }
        }

    }

}
