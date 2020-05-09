using com.csutil.datastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public static class ViewModelExtensions {

        public static Dictionary<string, FieldView> GetFieldViewMap<T>(this Presenter<T> self) {
            return self.targetView.GetComponentsInChildren<FieldView>().Filter(x => !x.fullPath.IsNullOrEmpty()).ToDictionary(x => x.fullPath, x => x);
        }

        public static T Get<T>(this Dictionary<string, FieldView> map, string name) where T : FieldView { return map[name] as T; }

        public static void AddOnValueChangedActionThrottled(this InputFieldView self, Action<string> onValueChanged) {
            ChangeTracker<string> changeTracker = new ChangeTracker<string>(null);
            self.input.AddOnValueChangedActionThrottled(newValue => {
                var regexValidator = self.GetComponent<RegexValidator>();
                if (regexValidator != null && !regexValidator.IsCurrentInputValid()) { return; }
                if (changeTracker.SetNewValue(newValue)) { onValueChanged(newValue); }
            });
        }

        public static void LinkViewToModel(this Dictionary<string, FieldView> self, string key, string text) { self[key].LinkToModel(text); }

        public static void LinkToModel(this FieldView self, string text) { self.mainLink.Get<Text>().text = text; }

        public static InputFieldView LinkViewToModel(this Dictionary<string, FieldView> self, string key, string val, Action<string> onNewVal) {
            return self.Get<InputFieldView>(key).LinkToModel(val, onNewVal);
        }

        public static InputFieldView LinkToModel(this InputFieldView self, string val, Action<string> onNewVal) {
            self.input.text = val;
            self.AddOnValueChangedActionThrottled(onNewVal);
            return self;
        }

        public static BoolFieldView LinkViewToModel(this Dictionary<string, FieldView> self, string key, bool val, Action<bool> onNewVal) {
            return self.Get<BoolFieldView>(key).LinkToModel(val, onNewVal);
        }

        public static BoolFieldView LinkToModel(this BoolFieldView self, bool val, Action<bool> onNewVal) {
            self.toggle.isOn = val;
            self.toggle.AddOnValueChangedAction(newVal => {
                onNewVal(newVal);
                return true;
            });
            return self;
        }

    }

}
