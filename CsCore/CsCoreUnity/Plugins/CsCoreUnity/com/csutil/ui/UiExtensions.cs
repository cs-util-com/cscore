using com.csutil.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil {

    public static class UiExtensions {

        public static void SetOnClickAction(this Button self, Action<GameObject> onClickAction) {
            if (self.onClick != null && self.onClick.GetPersistentEventCount() > 0) {
                Log.w("Overriding existing onClick action in " + self, self.gameObject);
            }
            self.onClick = new Button.ButtonClickedEvent();
            self.AddOnClickAction(onClickAction);
        }

        public static void AddOnClickAction(this Button self, Action<GameObject> onClickAction) {
            if (onClickAction != null) {
                self.onClick.AddListener(() => {
                    EventBus.instance.Publish(UiEvents.BUTTON_CLICKED, self);
                    try { onClickAction(self.gameObject); } catch (Exception e) { Log.e(e); }
                });
            }
        }

        public static void SetOnValueChangedAction(this Toggle self, Func<bool, bool> onValueChanged) {
            if (self.onValueChanged != null && self.onValueChanged.GetPersistentEventCount() > 0) {
                Log.w("Overriding old onValueChanged listener for toggle " + self, self.gameObject);
            }
            self.onValueChanged = new Toggle.ToggleEvent();
            AddOnValueChangedAction(self, onValueChanged);
        }

        public static void AddOnValueChangedAction(this Toggle self, Func<bool, bool> onValueChanged) {
            if (onValueChanged != null) {
                self.onValueChanged.AddListener((newCheckedState) => {
                    EventBus.instance.Publish(UiEvents.TOGGLE_CHANGED, self);
                    var changeAllowed = onValueChanged(newCheckedState);
                    if (!changeAllowed) { self.isOn = !newCheckedState; } // Undo the change
                });
            }
        }

        

        public static void SelectV2(this InputField self) {
            self.Select();
            self.ActivateInputField();
        }

    }

}
