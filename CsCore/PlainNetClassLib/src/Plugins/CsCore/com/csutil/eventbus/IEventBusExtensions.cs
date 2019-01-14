using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil {

    public static class IEventBusExtensions {

        /// <summary> Uses the eventHistory of the eventbus to check if the event was already published at least once </summary>
        /// <returns> true if the callback was instantly invoked </returns>
        public static bool SubscribeForOnePublishOrInstantInvokeIfInHistory(this IEventBus self, string eventName, Action callback) {
            if (self.eventHistory.Contains(eventName)) {
                callback.InvokeIfNotNull();
                return true;
            } else {
                self.SubscribeForOnePublish(new object(), eventName, callback);
                return false;
            }
        }

        /// <summary> 
        /// After the first event was published this will directly unsubscribe again so that 
        /// only one event will ever be received by the callback 
        /// </summary>
        public static void SubscribeForOnePublish(this IEventBus self, object subscriber, string eventName, Action callback) {
            self.Subscribe(subscriber, eventName, () => {
                self.Unsubscribe(subscriber, eventName);
                callback();
            });
        }
    }

}
