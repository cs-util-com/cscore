using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil {

    public static class IEventBusExtensions {

        /// <summary> 
        /// After the first event was published this will directly unsubscribe again so that 
        /// only one event will ever be received by the callback 
        /// </summary>
        public static void SubscribeOnMainThread(this IEventBus self, object subscriber, string eventName, Delegate callback) {
            self.Subscribe(subscriber, eventName, () => {
                MainThread.Invoke(() => {
                    callback.DynamicInvokeV2();
                });
            });
        }

    }

}
