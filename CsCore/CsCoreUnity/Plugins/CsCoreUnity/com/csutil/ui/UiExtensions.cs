using com.csutil.datastructures;
using com.csutil.model.immutable;
using com.csutil.ui;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.csutil {

    public static class UiExtensions {

        public static ViewStack GetViewStack(this GameObject gameObject) {
            var vs = gameObject.GetComponentInParents<ViewStack>();
            if (vs == null) { Log.e("Not part of a UI managed by a ViewStack", gameObject); }
            return vs;
        }

        public static Task SetOnClickAction(this Button self, Action<GameObject> onClickAction) {
            SetupOnClickEventObject(self);
            return self.AddOnClickAction(onClickAction);
        }

        public static Task<T> SetOnClickAction<T>(this Button self, Func<GameObject, T> onClickAction) {
            SetupOnClickEventObject(self);
            return self.AddOnClickAction(onClickAction);
        }

        private static void SetupOnClickEventObject(Button self) {
            if (self.onClick != null && self.onClick.GetPersistentEventCount() > 0) {
                Log.w("Overriding existing onClick action in " + self, self.gameObject);
            }
            self.onClick = new Button.ButtonClickedEvent(); // clear previous onClick listeners
        }

        public static Task AddOnClickAction(this Button self, Action<GameObject> onClickAction) {
            return AddOnClickAction(self, (go) => { onClickAction(go); return true; });
        }

        public static Task<T> AddOnClickAction<T>(this Button self, Func<GameObject, T> onClickFunc) {
            onClickFunc.ThrowErrorIfNull("Passed onClickFunc was null");
            var tcs = new TaskCompletionSource<T>();
            var originTrace = new StackTrace();
            self.onClick.AddListener(() => {
                EventBus.instance.Publish(EventConsts.catUi + UiEvents.BUTTON_CLICKED, self);
                try {
                    T res = onClickFunc(self.gameObject);
                    if (res is Task<T> asyncT) {
                        WaitForTaskSuccess(asyncT, originTrace, tcs).ContinueWithSameContext(wasSuccess => {
                            if (wasSuccess.Result) { tcs.TrySetResult(asyncT.Result); }
                        });
                    } else if (res is Task t) {
                        WaitForTaskSuccess(t, originTrace, tcs).ContinueWithSameContext(wasSuccess => {
                            if (wasSuccess.Result) { tcs.TrySetResult((T)(object)t); }
                        });
                    } else {
                        tcs.TrySetResult(res);
                    }
                }
                catch (Exception e) {
                    Log.e(e);
                    tcs.TrySetException(e);
                }
            });
            return tcs.Task;
        }

        private static async Task<bool> WaitForTaskSuccess<T>(Task task, StackTrace originTrace, TaskCompletionSource<T> tcs) {
            try {
                await task;
                return true;
            }
            catch (Exception e) {
                Log.e(e);
                if (task.IsCanceled) { tcs.TrySetCanceled(); }
                if (task.IsFaulted) { tcs.TrySetException(task.Exception); }
            }
            return false;
        }

        public static UnityAction<bool> SetOnValueChangedAction(this Toggle self, Func<bool, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for toggle " + self, self.gameObject);
            }
            self.onValueChanged = new Toggle.ToggleEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedAction(self, onValueChanged);
        }

        public static UnityAction<bool> AddOnValueChangedAction(this Toggle self, Func<bool, bool> onValueChanged) {
            if (onValueChanged != null) {
                var oldIsOn = self.isOn;
                UnityAction<bool> newListener = (newIsOn) => {
                    if (oldIsOn == newIsOn) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (!self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
                    if (!onValueChanged(newIsOn)) { // Change was rejected, revert UI:
                        self.isOn = oldIsOn;
                    } else { // Change was accepted:
                        oldIsOn = newIsOn;
                        EventBus.instance.Publish(EventConsts.catUi + UiEvents.TOGGLE_CHANGED, self, newIsOn);
                    }
                };
                self.onValueChanged.AddListener(newListener);
                return newListener;
            }
            return null;
        }

        public static UnityAction<float> SetOnValueChangedAction(this Slider self, Func<float, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for slider " + self, self.gameObject);
            }
            self.onValueChanged = new Slider.SliderEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedAction(self, onValueChanged);
        }

        public static UnityAction<float> AddOnValueChangedAction(this Slider self, Func<float, bool> onValueChanged) {
            if (onValueChanged != null) {
                var oldValue = self.value;
                UnityAction<float> newListener = (newValue) => {
                    if (SameValueAsBefore(oldValue, newValue, self.minValue, self.maxValue)) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (!self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
                    if (!onValueChanged(newValue)) { // Change was rejected, revert UI:
                        self.value = oldValue;
                    } else { // Change was accepted:
                        oldValue = newValue;
                        EventBus.instance.Publish(EventConsts.catUi + UiEvents.SLIDER_CHANGED, self, newValue);
                    }
                };
                self.onValueChanged.AddListener(newListener);
                return newListener;
            }
            return null;
        }

        private static bool SameValueAsBefore(float oldValue, float newValue, float minValue, float maxValue) {
            var absoluteChange = Mathf.Abs(newValue - oldValue);
            var fullSliderRange = maxValue - minValue;
            var percentageChanged = absoluteChange / fullSliderRange; // Values will be between 0 and 1
            return percentageChanged < 0.01; // If less then 1% change ignore it, UI glitch 
        }

        public static UnityAction<float> SetOnValueChangedActionThrottled(this Slider self, Action<float> onValueChanged, double delayInMs = 200) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new Slider.SliderEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedActionThrottled(self, onValueChanged, delayInMs);
        }

        public static UnityAction<float> AddOnValueChangedActionThrottled(this Slider self, Action<float> onValueChanged, double delayInMs = 1000) {
            EventHandler<float> action = (_, newFloat) => { onValueChanged(newFloat); };
            var throttledAction = action.AsThrottledDebounce(delayInMs, skipFirstEvent: true);
            return self.AddOnValueChangedAction((newValue) => {
                throttledAction(self, newValue);
                return true;
            });
        }

        public static bool ChangeWasTriggeredByUserThroughEventSystem(this Component self) {
            return EventSystem.current?.currentSelectedGameObject == self.gameObject;
        }

        public static UnityAction<string> SetOnValueChangedAction(this InputField self, Func<string, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedAction(self, onValueChanged);
        }

        public static UnityAction<string> AddOnValueChangedAction(this InputField self, Func<string, bool> onValueChanged) {
            if (onValueChanged != null) {
                var oldText = self.text;
                UnityAction<string> newListener = (newText) => {
                    if (newText == oldText) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (!self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
                    if (!onValueChanged(newText)) {
                        self.text = oldText;
                    } else {
                        oldText = newText;
                        EventBus.instance.Publish(EventConsts.catUi + UiEvents.INPUTFIELD_CHANGED, self, newText);
                    }
                };
                self.onValueChanged.AddListener(newListener);
                return newListener;
            }
            return null;
        }

        public static UnityAction<int> SetOnValueChangedAction(this Dropdown self, Func<int, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new Dropdown.DropdownEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedAction(self, onValueChanged);
        }

        public static UnityAction<int> AddOnValueChangedAction(this Dropdown self, Func<int, bool> onValueChanged) {
            if (onValueChanged != null) {
                var oldSelection = self.value;
                UnityAction<int> newListener = (newSection) => {
                    if (newSection == oldSelection) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (!self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
                    if (!onValueChanged(newSection)) {
                        self.value = oldSelection;
                    } else {
                        oldSelection = newSection;
                        EventBus.instance.Publish(EventConsts.catUi + UiEvents.DROPDOWN_CHANGED, self, newSection);
                    }
                };
                self.onValueChanged.AddListener(newListener);
                return newListener;
            }
            return null;
        }

        public static UnityAction<string> SetOnValueChangedActionThrottled(this InputField self, Action<string> onValueChanged, double delayInMs = 500) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedActionThrottled(self, onValueChanged, delayInMs);
        }

        public static UnityAction<string> AddOnValueChangedActionThrottled(this InputField self, Action<string> onValueChanged, double delayInMs = 500) {
            EventHandler<string> action = (_, newText) => { onValueChanged(newText); };
            var throttledAction = action.AsThrottledDebounce(delayInMs, skipFirstEvent: true);
            return self.AddOnValueChangedAction((newText) => {
                throttledAction(self, newText);
                return true;
            });
        }

        /// <summary> Sets focus on the input field </summary>
        public static void SelectV2(this InputField self) {
            self.Select();
            self.ActivateInputField();
        }

        public static void SetTextLocalizedWithNotify(this InputField self, string text) {
            self.SelectV2(); // Without this the change listeners are not triggered
            self.textLocalized(text);
        }

        public static void SubscribeToStateChanges<T, V>(this UnityEngine.Object self, IDataStore<T> store, Func<T, V> getSubState, Action<V> updateUi, bool triggerOnSubscribe = true) {
            updateUi(getSubState(store.GetState()));
            Action listener = null;
            listener = store.AddStateChangeListener(getSubState, newVal => {
                if (self.IsDestroyed()) { store.onStateChanged -= listener; return; }
                if (self is Behaviour b && !b.isActiveAndEnabled) { return; }
                if (self is GameObject go && !go.activeInHierarchy) { return; }
                updateUi(newVal);
            }, triggerOnSubscribe);
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

        public static int CalcCurrentMaxSortingOrderInLayer(this Canvas self) {
            var l = ResourcesV2.FindAllInScene<Canvas>().Filter(x => {
                if (x == self) { return false; } // Skip the input canvas
                if (!x.enabled) { return false; } // Only include currently active ones
                if (!x.gameObject.activeInHierarchy) { return false; }
                if (x.sortingLayerID != self.sortingLayerID) { return false; }
                var o = x.GetComponent<CanvasOrderOnTop>();
                if (o != null && o.excludeFromOrderCalc) { return false; }
                return true;
            });
            return l.Max(x => x.sortingOrder);
        }

    }

}
