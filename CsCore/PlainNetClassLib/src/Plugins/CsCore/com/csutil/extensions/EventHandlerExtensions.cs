using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class EventHandlerExtensions {

        /// <summary> 
        /// This will create an EventHandler where the first call is executed and the last call is executed but 
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        public static EventHandler<T> AsThrottledDebounce<T>(this EventHandler<T> self, double delayInMs) {
            bool currentlyThrottling = false;
            bool needsFinalCall = false;
            object threadLock = new object();
            Func<object, T, Task> asyncEventHandler = async (sender, eventArgs) => {
                lock (threadLock) {
                    if (currentlyThrottling) { needsFinalCall = true; return; }
                    currentlyThrottling = true;
                    self(sender, eventArgs);
                }
                await TaskV2.Delay(TimeSpan.FromMilliseconds(delayInMs));
                lock (threadLock) {
                    if (needsFinalCall) {
                        try { self(sender, eventArgs); } catch (Exception e) { Log.e(e); }
                    }
                    currentlyThrottling = false;
                    needsFinalCall = false;
                }
            };
            return (sender, eventArgs) => { asyncEventHandler(sender, eventArgs); };
        }

    }

}
