using System;
using com.csutil.model.immutable;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class UiExtensionsForReduxSubState {

        public static void SubscribeToStateChanges<T>(this Text self, SubState<T, string> subState) {
            subState.AddUnityStateChangeListener(self, newText => self.text = newText);
        }

        public static void SubscribeToStateChanges<T>(this InputField self, SubState<T, string> subState) {
            subState.AddUnityStateChangeListener(self, newText => self.text = newText);
        }

        public static void SubscribeToStateChanges<T>(this Toggle self, SubState<T, bool> subState) {
            subState.AddUnityStateChangeListener(self, newCheckedState => self.isOn = newCheckedState);
        }

        public static void SubscribeToStateChanges<T>(this Slider self, SubState<T, float> subState) {
            subState.AddUnityStateChangeListener(self, newText => self.value = newText);
        }

        public static void AddUnityStateChangeListener<T, S>(this SubState<T, S> subState, UnityEngine.Object unityObject, Action<S> updateUi, bool triggerOnSubscribe = true, bool eventsAlwaysInMainThread = true) {
            updateUi(subState.GetState());
            Wrapper w = new Wrapper();
            w.stateChangeListener = () => {
                var newVal = subState.GetState();
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { SubscribeToStateChanges_OnChanged(unityObject, subState, updateUi, w, newVal); });
                } else {
                    SubscribeToStateChanges_OnChanged(unityObject, subState, updateUi, w, newVal);
                }
            };
            subState.onStateChanged += w.stateChangeListener;
        }

        private class Wrapper {
            public Action stateChangeListener;
        }

        private static void SubscribeToStateChanges_OnChanged<T, S>(UnityEngine.Object unityObject, SubState<T, S> subState, Action<S> updateUi, Wrapper w, S newVal) {
            if (unityObject.IsDestroyed()) {
                subState.onStateChanged -= w.stateChangeListener;
                return;
            }
            UiExtensionsForReduxStore.OnStateChangedForUnity(updateUi, newVal, unityObject);
        }

        public static SubState<T, S> GetSubStateForUnity<T, S>(this IDataStore<T> store, UnityEngine.Object context, Func<T, S> getSubState, bool eventsAlwaysInMainThread = true) {
            SubState<T, S> subState = store.GetSubState(getSubState);
            var listenerInStore = store.AddStateChangeListener((_) => subState.GetState(), newSubState => {
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { OnSubstateChangedForUnity(subState, newSubState, context); });
                } else {
                    OnSubstateChangedForUnity(subState, newSubState, context);
                }
            }, triggerInstantToInit: true);
            subState.RemoveFromParent = () => { store.onStateChanged -= listenerInStore; };
            return subState;
        }

        public static SubState<T, SubSub> GetSubStateForUnity<T, S, SubSub>(this SubState<T, S> parentSubState, UnityEngine.Object context, Func<S, SubSub> getSubState, bool eventsAlwaysInMainThread = true) {
            var subState = parentSubState.GetSubState(getSubState);
            var ownListenerInParent = parentSubState.AddStateChangeListener(getSubState, newSubState => {
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { OnSubstateChangedForUnity(subState, newSubState, context); });
                } else {
                    OnSubstateChangedForUnity(subState, newSubState, context);
                }
            });
            subState.RemoveFromParent = () => { parentSubState.onStateChanged -= ownListenerInParent; };
            return subState;
        }

        private static void OnSubstateChangedForUnity<T, S>(SubState<T, S> subState, S newVal, UnityEngine.Object context) {
            if (context.IsDestroyed()) {
                subState.DisposeV2();
                return;
            }
            if (context is Behaviour b && !b.isActiveAndEnabled) { return; }
            if (context is GameObject go && !go.activeInHierarchy) { return; }
            subState.TriggerOnSubstateChanged(newVal);
        }

    }

    public static class UiExtensionsForReduxStore {

        private class Wrapper {
            public Action stateChangeListener;
        }

        public static void SubscribeToStateChanges<T, V>(this UnityEngine.Object self, IDataStore<T> store, Func<T, V> getSubState, Action<V> updateUi, bool triggerOnSubscribe = true, bool eventsAlwaysInMainThread = true) {
            updateUi(getSubState(store.GetState()));
            Wrapper w = new Wrapper();
            w.stateChangeListener = store.AddStateChangeListener(getSubState, newVal => {
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { SubscribeToStateChanges_OnChanged(self, store, updateUi, w, newVal); });
                } else {
                    SubscribeToStateChanges_OnChanged(self, store, updateUi, w, newVal);
                }
            }, triggerOnSubscribe);
        }

        private static void SubscribeToStateChanges_OnChanged<T, V>(UnityEngine.Object self, IDataStore<T> store, Action<V> updateUi, Wrapper w, V newVal) {
            if (self.IsDestroyed()) {
                store.onStateChanged -= w.stateChangeListener;
                return;
            }
            OnStateChangedForUnity(updateUi, newVal, self);
        }

        internal static void OnStateChangedForUnity<V>(Action<V> onStateChanged, V newVal, UnityEngine.Object context) {
            if (context is Behaviour b && !b.isActiveAndEnabled) { return; }
            if (context is GameObject go && !go.activeInHierarchy) { return; }
            onStateChanged(newVal);
        }

        public static void SubscribeToStateChanges<T>(this Text self, IDataStore<T> store, Func<T, string> getSubState) {
            self.SubscribeToStateChanges(store, getSubState, newText => self.text = newText);
        }

        public static void SubscribeToStateChanges<T>(this InputField self, IDataStore<T> store, Func<T, string> getSubState) {
            self.SubscribeToStateChanges(store, getSubState, newText => self.text = newText);
        }

        public static void SubscribeToStateChanges<T>(this Toggle self, IDataStore<T> store, Func<T, bool> getSubState) {
            self.SubscribeToStateChanges(store, getSubState, newCheckedState => self.isOn = newCheckedState);
        }

        public static void SubscribeToStateChanges<T>(this Slider self, IDataStore<T> store, Func<T, float> getSubState) {
            self.SubscribeToStateChanges(store, getSubState, newText => self.value = newText);
        }

    }

    [Obsolete("SubListeners class was replaced by SubState")]
    public static class OldUiExtensionsForReduxSubListeners {

        [Obsolete("Use GetSubStateForUnity instead")]
        public static SubListeners<S> NewSubStateListenerForUnity<T, S>(this IDataStore<T> self, UnityEngine.Object context, Func<T, S> getSubState, bool eventsAlwaysInMainThread = true) {
            var subListener = new SubListeners<S>(getSubState(self.GetState()));
            var ownListenerInParent = self.AddStateChangeListener(getSubState, newSubState => {
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { OnSubstateChangedForUnity(subListener, newSubState, context); });
                } else {
                    OnSubstateChangedForUnity(subListener, newSubState, context);
                }
            });
            subListener.SetUnregisterInParentAction(() => { self.onStateChanged -= ownListenerInParent; });
            return subListener;
        }

        [Obsolete("Use GetSubStateForUnity instead", true)]
        public static SubListeners<S> NewSubStateListenerForUnity<T, S>(this SubListeners<T> self, UnityEngine.Object context, Func<T, S> getSubState, bool eventsAlwaysInMainThread = true) {
            var subListener = new SubListeners<S>(getSubState(self.latestSubState));
            var ownListenerInParent = self.AddStateChangeListener(getSubState, newSubState => {
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { OnSubstateChangedForUnity(subListener, newSubState, context); });
                } else {
                    OnSubstateChangedForUnity(subListener, newSubState, context);
                }
            });
            subListener.SetUnregisterInParentAction(() => { self.innerListeners -= ownListenerInParent; });
            return subListener;
        }

        private static void OnSubstateChangedForUnity<V>(SubListeners<V> onStateChanged, V newVal, UnityEngine.Object context) {
            if (context.IsDestroyed()) {
                onStateChanged.UnregisterFromParent();
                return;
            }
            if (context is Behaviour b && !b.isActiveAndEnabled) { return; }
            if (context is GameObject go && !go.activeInHierarchy) { return; }
            onStateChanged.OnSubstateChanged(newVal);
        }

    }

}