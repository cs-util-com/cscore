using com.csutil.ui;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class UiExtensions {

        public static ViewStack GetViewStack(this GameObject gameObject) {
            return gameObject.GetComponentInParent<ViewStack>();
        }

        public static void SetOnClickAction(this Button self, Action<GameObject> onClickAction) {
            if (self.onClick != null && self.onClick.GetPersistentEventCount() > 0) {
                Log.w("Overriding existing onClick action in " + self, self.gameObject);
            }
            self.onClick = new Button.ButtonClickedEvent(); // clear previous onClick listeners
            self.AddOnClickAction(onClickAction);
        }

        public static void AddOnClickAction(this Button self, Action<GameObject> onClickAction) {
            if (onClickAction != null) {
                var originTrace = new StackTrace();
                self.onClick.AddListener(() => {
                    EventBus.instance.Publish(UiEvents.BUTTON_CLICKED, self);
                    try { onClickAction(self.gameObject); }
                    catch (Exception e) { Log.e(e + " at " + originTrace); }
                });
            }
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
                    EventBus.instance.Publish(UiEvents.TOGGLE_CHANGED, self);
                    if (!onValueChanged(newIsOn)) { self.isOn = oldIsOn; } else { oldIsOn = newIsOn; }
                });
            }
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
                    EventBus.instance.Publish(UiEvents.INPUTFIELD_CHANGED, self);
                    if (!onValueChanged(newText)) { self.text = oldText; } else { oldText = newText; }
                });
            }
        }

        public static void SelectV2(this InputField self) {
            self.Select();
            self.ActivateInputField();
        }

    }

}
