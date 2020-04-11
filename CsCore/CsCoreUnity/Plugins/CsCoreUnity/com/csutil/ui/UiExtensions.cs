using com.csutil.model.immutable;
using com.csutil.ui;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.csutil {

    public static class UiExtensions {

        public static ViewStack GetViewStack(this GameObject gameObject) {
            return gameObject.GetComponentInParents<ViewStack>();
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
            var tcs = new TaskCompletionSource<T>();
            if (onClickFunc != null) {
                var originTrace = new StackTrace();
                self.onClick.AddListener(() => {
                    EventBus.instance.Publish(UiEvents.BUTTON_CLICKED, self);
                    try {
                        T res = onClickFunc(self.gameObject);
                        if (res is Task<T> asyncT) {
                            asyncT.ContinueWith(task => tcs.TrySetResult(task.Result));
                        } else if (res is Task t) {
                            t.ContinueWith(task => tcs.TrySetResult((T)(object)task));
                        } else {
                            tcs.TrySetResult(res);
                        }
                    }
                    catch (Exception e) {
                        Log.e(e + " at " + originTrace);
                        tcs.TrySetException(e);
                    }
                });
            }
            return tcs.Task;
        }

        public static void SetOnValueChangedAction(this Toggle self, Func<bool, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for toggle " + self, self.gameObject);
            }
            self.onValueChanged = new Toggle.ToggleEvent(); // clear previous onValueChanged listeners
            AddOnValueChangedAction(self, onValueChanged);
        }

        public static void AddOnValueChangedAction(this Toggle self, Func<bool, bool> onValueChanged) {
            if (onValueChanged != null) {
                var oldIsOn = self.isOn;
                self.onValueChanged.AddListener((newIsOn) => {
                    if (oldIsOn == newIsOn) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (!self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
                    if (!onValueChanged(newIsOn)) { // Change was rejected, revert UI:
                        self.isOn = oldIsOn;
                    } else { // Change was accepted:
                        oldIsOn = newIsOn;
                        EventBus.instance.Publish(UiEvents.TOGGLE_CHANGED, self, newIsOn);
                    }
                });
            }
        }

        public static bool ChangeWasTriggeredByUserThroughEventSystem(this Component self) {
            return EventSystem.current?.currentSelectedGameObject == self.gameObject;
        }

        public static void SetOnValueChangedAction(this InputField self, Func<string, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            AddOnValueChangedAction(self, onValueChanged);
        }

        public static void AddOnValueChangedAction(this InputField self, Func<string, bool> onValueChanged) {
            if (onValueChanged != null) {
                var oldText = self.text;
                self.onValueChanged.AddListener((newText) => {
                    if (newText == oldText) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (!self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
                    if (!onValueChanged(newText)) {
                        self.text = oldText;
                    } else {
                        oldText = newText;
                        EventBus.instance.Publish(UiEvents.INPUTFIELD_CHANGED, self, newText);
                    }
                });
            }
        }

        public static void SetOnValueChangedActionThrottled(this InputField self, Action<string> onValueChanged, double delayInMs = 1000) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            AddOnValueChangedActionThrottled(self, onValueChanged, delayInMs);
        }

        public static void AddOnValueChangedActionThrottled(this InputField self, Action<string> onValueChanged, double delayInMs = 1000) {
            EventHandler<string> action = (input, newText) => { onValueChanged(newText); };
            var throttledAction = action.AsThrottledDebounce(delayInMs);
            self.AddOnValueChangedAction((newText) => {
                throttledAction(self, newText);
                return true;
            });
        }

        public static void SelectV2(this InputField self) {
            self.Select();
            self.ActivateInputField();
        }

        public static void SubscribeToStateChanges<T, V>(this Behaviour self, IDataStore<T> store, Func<T, V> getSubState, Action<V> updateUi) {
            updateUi(getSubState(store.GetState()));
            Action listener = null;
            listener = store.AddStateChangeListener(getSubState, newVal => {
                Log.d("StateChangeRelevantForBehaviour=" + self, "newVal=" + newVal); // TODO remove me
                if (self.IsDestroyed()) {
                    store.onStateChanged -= listener;
                } else if (self.isActiveAndEnabled) {
                    updateUi(newVal);
                }
            });
        }

        public static int CalcCurrentMaxSortingOrderInLayer(this Canvas self) {
            var l = ResourcesV2.FindAllInScene<Canvas>().Filter(x =>
                x.gameObject.activeInHierarchy && x.enabled && x != self && x.sortingLayerID == self.sortingLayerID
            );
            return l.Max(x => x.sortingOrder);
        }

    }

    public static class AppFlowUiExtensions {

        public static void ActivateViewStackTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.SHOW_VIEW, (GameObject view) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SHOW_VIEW + "_" + view.name, view);
            });
            EventBus.instance.Subscribe(self, EventConsts.SWITCH_BACK_TO_LAST_VIEW, (string currentViewName, GameObject lastView) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SWITCH_BACK_TO_LAST_VIEW + "_" + currentViewName + "->" + lastView.name, lastView);
            });
            EventBus.instance.Subscribe(self, EventConsts.SWITCH_TO_NEXT_VIEW, (GameObject currentView, GameObject nextView) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SWITCH_TO_NEXT_VIEW + "_" + currentView.name + "->" + nextView.name, currentView, nextView);
            });
        }

        public static void ActivateUiEventTracking(this IAppFlow self) {

            // Button UI tracking:
            EventBus.instance.Subscribe(self, UiEvents.BUTTON_CLICKED, (Button button) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.BUTTON_CLICKED + "_" + button, button);
            });

            // Toggle UI tracking:
            EventBus.instance.Subscribe(self, UiEvents.TOGGLE_CHANGED, (Toggle toggle, bool isChecked) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.TOGGLE_CHANGED + "_" + toggle + "_" + isChecked, toggle, isChecked);
            });

            // InputField UI tracking:
            EventHandler<string> action = (input, newText) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.INPUTFIELD_CHANGED + "_" + input, input);
            };
            var delayedAction = action.AsThrottledDebounce(delayInMs: 1900, skipFirstEvent: true);
            EventBus.instance.Subscribe(self, UiEvents.INPUTFIELD_CHANGED, (InputField input, string newText) => {
                delayedAction(input, newText);
            });
        }

    }

}
