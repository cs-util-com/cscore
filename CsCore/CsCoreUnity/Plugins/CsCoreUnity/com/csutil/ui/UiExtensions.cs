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
            if (onClickAction != null) { self.onClick.AddListener(() => onClickAction(self.gameObject)); }
        }
    }

}
