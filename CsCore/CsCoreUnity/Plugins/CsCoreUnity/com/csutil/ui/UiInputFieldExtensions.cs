using System;
using com.csutil.ui;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil {
    
    public static class UiInputFieldExtensions {
        
        public static UnityAction<string> SetOnValueChangedActionThrottled(this TMP_InputField self, Action<string> onValueChanged, double delayInMs = 500) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new TMP_InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedActionThrottled(self, onValueChanged, delayInMs);
        }
        
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static UnityAction<string> SetOnValueChangedActionThrottled(this InputField self, Action<string> onValueChanged, double delayInMs = 500) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedActionThrottled(self, onValueChanged, delayInMs);
        }

        public static UnityAction<string> AddOnValueChangedActionThrottled(this TMP_InputField self, Action<string> onValueChanged, double delayInMs = 500) {
            EventHandler<string> action = (_, newText) => { onValueChanged(newText); };
            var throttledAction = action.AsThrottledDebounce(delayInMs, skipFirstEvent: true);
            return self.AddOnValueChangedAction((newText) => {
                throttledAction(self, newText);
                return true;
            });
        }
        
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static UnityAction<string> AddOnValueChangedActionThrottled(this InputField self, Action<string> onValueChanged, double delayInMs = 500) {
            EventHandler<string> action = (_, newText) => { onValueChanged(newText); };
            var throttledAction = action.AsThrottledDebounce(delayInMs, skipFirstEvent: true);
            return self.AddOnValueChangedAction((newText) => {
                throttledAction(self, newText);
                return true;
            });
        }

        public static void SelectV2(this TMP_InputField self) {
            self.Select();
            self.ActivateInputField();
        }
        
        /// <summary> Sets focus on the input field </summary>
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static void SelectV2(this InputField self) {
            self.Select();
            self.ActivateInputField();
        }

        public static void SetTextLocalizedWithNotify(this TMP_InputField self, string text) {
            self.SelectV2(); // Without this the change listeners are not triggered
            self.textLocalized(text);
        }
        
        /// <summary> Sets the input text localized which will notify all UI listeners </summary>
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static void SetTextLocalizedWithNotify(this InputField self, string text) {
            self.SelectV2(); // Without this the change listeners are not triggered
            self.textLocalized(text);
        }
        
        public static UnityAction<string> SetOnValueChangedAction(this TMP_InputField self, Func<string, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new TMP_InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedAction(self, onValueChanged);
        }
        
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static UnityAction<string> SetOnValueChangedAction(this InputField self, Func<string, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for input field " + self, self.gameObject);
            }
            self.onValueChanged = new InputField.OnChangeEvent(); // clear previous onValueChanged listeners
            return AddOnValueChangedAction(self, onValueChanged);
        }

        public static UnityAction<string> AddOnValueChangedAction(this TMP_InputField self, Func<string, bool> onValueChanged, bool skipChangesByLogic = true) {
            if (self.IsNullOrDestroyed()) {
                throw new ArgumentNullException("self (InputField)");
            }
            if (onValueChanged != null) {
                var oldText = self.text;
                UnityAction<string> newListener = (newText) => {
                    if (newText == oldText) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (skipChangesByLogic && !self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
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
        
        [Obsolete("Use TMP_InputField instead of InputField")]
        public static UnityAction<string> AddOnValueChangedAction(this InputField self, Func<string, bool> onValueChanged, bool skipChangesByLogic = true) {
            if (self.IsNullOrDestroyed()) {
                throw new ArgumentNullException("self (InputField)");
            }
            if (onValueChanged != null) {
                var oldText = self.text;
                UnityAction<string> newListener = (newText) => {
                    if (newText == oldText) { return; }
                    // Ignore event event if it was triggered through code, only fire for actual user input:
                    if (skipChangesByLogic && !self.ChangeWasTriggeredByUserThroughEventSystem()) { return; }
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
        
    }
    
}