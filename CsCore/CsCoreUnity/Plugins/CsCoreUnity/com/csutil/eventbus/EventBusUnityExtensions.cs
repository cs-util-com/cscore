using com.csutil.eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class EventBusUnityExtensions {

        public static void Subscribe(this Behaviour b, string eventName, Action callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }
        public static void Subscribe<T>(this Behaviour b, string eventName, Action<T> callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }
        public static void Subscribe<T, U>(this Behaviour b, string eventName, Action<T, U> callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }
        public static void Subscribe<T, U, V>(this Behaviour b, string eventName, Action<T, U, V> callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }
        public static void Subscribe<T>(this Behaviour b, string eventName, Func<T> callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }
        public static void Subscribe<T, U>(this Behaviour b, string eventName, Func<T, U> callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }
        public static void Subscribe<T, U, V>(this Behaviour b, string eventName, Func<T, U, V> callback) {
            EventBus.instance.SubscribeMono(b, eventName, callback);
        }

        public static void SubscribeMono(this IEventBus self, Behaviour b, string eventName, Action callback) {
            self.Subscribe(b, eventName, () => { CallbackIfActive(self, b, callback); });
        }
        public static void SubscribeMono<T>(this IEventBus self, Behaviour b, string eventName, Action<T> callback) {
            self.Subscribe(b, eventName, (T t) => { CallbackIfActive(self, b, callback, t); });
        }
        public static void SubscribeMono<T, V>(this IEventBus self, Behaviour b, string eventName, Action<T, V> callback) {
            self.Subscribe(b, eventName, (T t, V v) => { CallbackIfActive(self, b, callback, t, v); });
        }
        public static void SubscribeMono<T, U, V>(this IEventBus self, Behaviour b, string eventName, Action<T, U, V> callback) {
            self.Subscribe(b, eventName, (T t, U u, V v) => { CallbackIfActive(self, b, callback, t, u, v); });
        }
        public static void SubscribeMono<T>(this IEventBus self, Behaviour b, string eventName, Func<T> callback) {
            self.Subscribe(b, eventName, () => { return CallbackIfActive(self, b, callback); });
        }
        public static void SubscribeMono<T, U>(this IEventBus self, Behaviour b, string eventName, Func<T, U> callback) {
            self.Subscribe(b, eventName, (T t) => { return CallbackIfActive(self, b, callback, t); });
        }
        public static void SubscribeMono<T, U, V>(this IEventBus self, Behaviour b, string eventName, Func<T, U, V> callback) {
            self.Subscribe(b, eventName, (T t, U u) => { return CallbackIfActive(self, b, callback, t, u); });
        }

        private static object CallbackIfActive(IEventBus self, Behaviour m, Delegate l, params object[] args) {
            if (m == null) {
                self.UnsubscribeAll(m);
            } else if (m.gameObject.activeInHierarchy && m.enabled) {
                return l.DynamicInvokeV2(args);
            }
            return null;
        }

        public static void Subscribe(this GameObject go, string eventName, Action callback) {
            EventBus.instance.SubscribeGameObject(go, eventName, callback);
        }
        public static void Subscribe<T>(this GameObject go, string eventName, Action<T> callback) {
            EventBus.instance.SubscribeGameObject(go, eventName, callback);
        }
        public static void Subscribe<T, V>(this GameObject go, string eventName, Action<T, V> callback) {
            EventBus.instance.SubscribeGameObject(go, eventName, callback);
        }
        public static void Subscribe<T, U, V>(this GameObject go, string eventName, Action<T, U, V> callback) {
            EventBus.instance.SubscribeGameObject(go, eventName, callback);
        }

        public static void SubscribeGameObject(this IEventBus self, GameObject go, string eventName, Action callback) {
            self.Subscribe(go, eventName, () => { CallbackIfActive(self, go, callback); });
        }
        public static void SubscribeGameObject<T>(this IEventBus self, GameObject go, string eventName, Action<T> callback) {
            self.Subscribe(go, eventName, (T t) => { CallbackIfActive(self, go, callback, t); });
        }
        public static void SubscribeGameObject<T, V>(this IEventBus self, GameObject go, string eventName, Action<T, V> callback) {
            self.Subscribe(go, eventName, (T t, V v) => { CallbackIfActive(self, go, callback, t, v); });
        }
        public static void SubscribeGameObject<T, U, V>(this IEventBus self, GameObject go, string eventName, Action<T, U, V> callback) {
            self.Subscribe(go, eventName, (T t, U u, V v) => { CallbackIfActive(self, go, callback, t, u, v); });
        }

        private static void CallbackIfActive(IEventBus self, GameObject go, Delegate l, params object[] args) {
            try {
                if (go == null) {
                    self.UnsubscribeAll(go);
                } else if (go.activeInHierarchy) {
                    l.DynamicInvokeV2(args);
                }
            }
            catch (Exception e) { Log.e(e); }
        }

        /// <summary> This will ensure that the subscribe callback happens on the main thread </summary>
        public static void SubscribeOnMainThread(this IEventBus self, object subscriber, string eventName, Action callback) {
            self.Subscribe(subscriber, eventName, () => {
                MainThread.Invoke(() => {
                    callback.InvokeIfNotNull();
                });
            });
        }

    }

}
