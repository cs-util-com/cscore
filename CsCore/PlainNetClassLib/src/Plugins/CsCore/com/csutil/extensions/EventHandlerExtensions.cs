using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class EventHandlerExtensions {

        /// <summary> 
        /// This will create an action where the first call is executed and the last call is executed but 
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        public static EventHandler<T> AsThrottledDebounce<T>(this EventHandler<T> self, double delayInMs) {
            bool currentlyThrottling = false;
            bool needsFinalCall = false;
            object threadLock = new object();
            return (sender, eventArgs) => {
                lock (threadLock) {
                    if (currentlyThrottling) { needsFinalCall = true; return; }
                    currentlyThrottling = true;
                    self(sender, eventArgs);
                }
                Task.Delay(TimeSpan.FromMilliseconds(delayInMs)).ContinueWith(_ => {
                    lock (threadLock) {
                        if (needsFinalCall) { self(sender, eventArgs); }
                        currentlyThrottling = false;
                        needsFinalCall = false;
                    }
                });
            };
        }

    }

}
