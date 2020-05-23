using com.csutil.datastructures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public static class ViewModelExtensions {

        public static Dictionary<string, FieldView> GetFieldViewMap(this GameObject self) {
            return self.GetComponentsInChildren<FieldView>().Filter(x => !x.fullPath.IsNullOrEmpty()).ToDictionary(x => x.fullPath, x => x);
        }

        public static T Get<T>(this Dictionary<string, FieldView> map, string name) where T : FieldView { return map[name] as T; }

        public static void AddOnValueChangedActionThrottled(this InputFieldView self, Action<string> onValueChanged) {
            ChangeTracker<string> changeTracker = new ChangeTracker<string>(null);
            self.input.AddOnValueChangedActionThrottled(newValue => {
                if (self.IsDestroyed()) { return; }
                var regexValidator = self.GetComponent<RegexValidator>();
                if (regexValidator != null && !regexValidator.CheckIfCurrentInputValid()) { return; }
                if (changeTracker.SetNewValue(newValue)) { onValueChanged(newValue); }
            });
        }

        public static void LinkViewToModel(this Dictionary<string, FieldView> self, string key, string text) { self[key].LinkToModel(text); }

        public static void LinkToModel(this FieldView self, string text) { self.mainLink.Get<Text>().text = text; }

        public static InputFieldView LinkViewToModel(this Dictionary<string, FieldView> self, string key, string val, Action<string> onNewVal) {
            return self.Get<InputFieldView>(key).LinkToModel(val, onNewVal);
        }

        public static InputFieldView LinkToModel(this InputFieldView self, string val, Action<string> onNewVal) {
            if (val != null) { self.input.text = val; }
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

        /// <summary> 
        /// Converts the passed model to JSON, lets the user edit it and returned a parsed back clone with all changes 
        /// made by the user, so that this new state can be stored or the changed fields can be calculated via MergeJson.GetDiff()
        /// </summary>
        /// <param name="model"> The model that should be shown in the UI (has to fit the loaded view model UI) </param>
        /// <param name="userSavedChanges"> 
        /// A task that should be set to completed once the user is finished with the UI, e.g. when he presses the save button 
        /// </param>
        /// <returns> The modified model after the passed userSavedChanges-Task is completed </returns>
        public static async Task<T> LoadViaJsonIntoView<T>(this Presenter<JObject> self, T model, Task userSavedChanges) {
            JObject json = JObject.Parse(JsonWriter.GetWriter().Write(model));
            await self.LoadModelIntoView(json);
            await userSavedChanges;
            return JsonReader.GetReader().Read<T>(json.ToString());
        }

        public static bool IsInChildObject(this FieldView self) { return self.fieldName != self.fullPath; }

    }

}
