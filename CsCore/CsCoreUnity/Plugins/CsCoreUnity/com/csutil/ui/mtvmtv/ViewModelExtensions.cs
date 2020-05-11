using com.csutil.datastructures;
using com.csutil.model.mtvmtv;
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

        public static async Task<T> LoadViaJsonIntoView<T>(this Presenter<JObject> self, T model, Task userSavesChanges) {
            JObject json = JObject.Parse(JsonWriter.GetWriter().Write(model));
            await self.LoadModelIntoView(json);
            await userSavesChanges;
            return JsonReader.GetReader().Read<T>(json.ToString());
        }


        public static bool IsInChildObject(this FieldView self) { return self.fieldName != self.fullPath; }

        public static bool LinkToJsonModel(this FieldView self, JObject root) {
            string name = self.fieldName;
            var model = self.GetChildJObjFrom(root);
            if (self is InputFieldView i) {
                i.LinkToModel("" + model?[name], newVal => {
                    self.CreateChildJObjIfNeeded(root);
                    self.GetChildJObjFrom(root)[name] = self.field.ParseToJValue(newVal);
                });
                return true;
            }
            if (self is BoolFieldView b) {
                bool val = (model?[name] as JValue)?.Value<bool>() == true;
                b.LinkToModel(val, newB => {
                    self.CreateChildJObjIfNeeded(root);
                    self.GetChildJObjFrom(root)[name] = new JValue(newB);
                });
                return true;
            }
            if (self.field.readOnly == true) {
                self.LinkToModel("" + model?[name]);
                return true;
            }
            return false;
        }

        public static void ShowChildModelInNewScreen(this RecursiveModelField self, JObject root, GameObject currentScreen) {
            self.openButton.SetOnClickAction(async delegate {
                var newScreen = await self.NewViewFromViewModel();
                var viewStack = currentScreen.GetViewStack();
                viewStack.ShowView(newScreen, currentScreen);
                var p = new JObjectPresenter();
                p.targetView = newScreen;
                var model = self.GetChildJObjFrom(root)[self.fieldName] as JObject;
                AssertV2.NotNull(model, "model");
                await p.LoadModelIntoView(model);
                viewStack.SwitchBackToLastView(newScreen);
            }).LogOnError();
        }

        private static void CreateChildJObjIfNeeded(this FieldView self, JObject rootModel) {
            if (self.IsInChildObject()) { // Navigate down to the correct child JObject
                string[] parents = self.fullPath.Split(".");
                foreach (var p in parents.Take(parents.Length - 1)) {
                    if (rootModel[p] == null) { rootModel[p] = new JObject(); }
                    rootModel = rootModel[p] as JObject;
                    AssertV2.NotNull(rootModel, $"rootModel (p={p}");
                }
            }
        }

        public static JObject GetChildJObjFrom(this FieldView self, JObject rootModel) {
            if (self.IsInChildObject()) { // Navigate down to the correct child JObject
                string[] parents = self.fullPath.Split(".");
                foreach (var p in parents.Take(parents.Length - 1)) {
                    rootModel = rootModel[p] as JObject;
                }
            }
            return rootModel;
        }
    }

}
