using System;
using com.csutil.model.immutable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class UiExtensionsForReduxSubState {

        public static void SubscribeToStateChanges<T, S>(this TMP_Text self, SubState<T, S> subState, Func<S, string> getSubState) {
            SubscribeToStateChanges(self, subState, getSubState, newText => self.text = newText);
        }
        
        [Obsolete("Use TMP_Text instead of Text")]
        public static void SubscribeToStateChanges<T, S>(this Text self, SubState<T, S> subState, Func<S, string> getSubState) {
            SubscribeToStateChanges(self, subState, getSubState, newText => self.text = newText);
        }

        public static void SetSubState<T, S>(this TMP_InputField self, SubState<T, S> subState, Func<S, string> getSubState, Action<string> onValueChanged) {
            if (self.IsNullOrDestroyed()) { throw new ArgumentNullException("self(InputField) must not be null!"); }
            self.SubscribeToStateChanges(subState, getSubState);
            self.AddOnValueChangedActionThrottled(onValueChanged);
        }
        
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static void SetSubState<T, S>(this InputField self, SubState<T, S> subState, Func<S, string> getSubState, Action<string> onValueChanged) {
            if (self.IsNullOrDestroyed()) { throw new ArgumentNullException("self(InputField) must not be null!"); }
            self.SubscribeToStateChanges(subState, getSubState);
            self.AddOnValueChangedActionThrottled(onValueChanged);
        }

        public static void SubscribeToStateChanges<T, S>(this TMP_InputField self, SubState<T, S> subState, Func<S, string> getSubState) {
            SubscribeToStateChanges(self, subState, getSubState, newText => self.text = newText);
        }
        
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static void SubscribeToStateChanges<T, S>(this InputField self, SubState<T, S> subState, Func<S, string> getSubState) {
            SubscribeToStateChanges(self, subState, getSubState, newText => self.text = newText);
        }

        public static void SubscribeToStateChanges<T, S>(this Toggle self, SubState<T, S> subState, Func<S, bool> getSubState) {
            SubscribeToStateChanges(self, subState, getSubState, newCheckedState => self.isOn = newCheckedState);
        }

        public static void SubscribeToStateChanges<T, S>(this Slider self, SubState<T, S> subState, Func<S, float> getSubState) {
            SubscribeToStateChanges(self, subState, getSubState, newText => self.value = newText);
        }

        public static SubState<T, Sub> GetSubStateForUnity<T, Sub>(this IDataStore<T> store, UnityEngine.Object context, Func<T, Sub> getSubState, bool eventsAlwaysInMainThread = true) {
            SubState<T, Sub> subState = store.GetSubState(getSubState);
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

        private static void SubscribeToStateChanges<T, S, Sub>(UnityEngine.Object ui, SubState<T, S> sub, Func<S, Sub> getSubState, Action<Sub> updateUi) {
            sub.GetSubStateForUnity(ui, getSubState, updateUi);
        }
        
        public static SubState<T, SubSub> GetSubStateForUnity<T, Sub, SubSub>(this SubState<T, Sub> parentSubState, UnityEngine.Object context, Func<Sub, SubSub> getSubState, Action<SubSub> onChanged, bool eventsAlwaysInMainThread = true) {
            if (context.IsNullOrDestroyed()) {
                throw new ArgumentNullException("The Unity context object must not be null!");
            }
            var subState = parentSubState.GetSubState(getSubState);
            var ownListenerInParent = parentSubState.AddStateChangeListener(getSubState, newSubState => {
                if (eventsAlwaysInMainThread) {
                    MainThread.Invoke(() => { OnSubstateChangedForUnity(subState, newSubState, context); });
                } else {
                    OnSubstateChangedForUnity(subState, newSubState, context);
                }
            });
            subState.RemoveFromParent = () => { parentSubState.onStateChanged -= ownListenerInParent; };
            subState.AddStateChangeListener(onChanged);
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