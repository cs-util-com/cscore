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

        public static bool LinkToJsonModel(this FieldView self, JObject root) {
            JToken value = GetFieldJModel(self, root);
            if (self is EnumFieldView enumFieldView && value?.Type == JTokenType.Integer) {
                int posInEnum = int.Parse("" + value);
                var enumValues = self.field.contentEnum;
                enumFieldView.LinkToModel(enumValues[posInEnum], newVal => {
                    var newPosInEnum = Array.FindIndex(enumValues, x => x == newVal);
                    SetNewJValueInModel(self, root, new JValue(newPosInEnum));
                });
                return true;
            }
            if (self is InputFieldView inputFieldView) {
                inputFieldView.LinkToModel("" + value, newVal => {
                    try {
                        var newJVal = self.field.ParseToJValue(newVal);
                        SetNewJValueInModel(self, root, newJVal);
                    } // Ignore errors like e.g. FormatException when "" is parsed to int:
                    catch (FormatException e) { Log.w("" + e, self.gameObject); }
                });
                return true;
            }
            if (self is BoolFieldView boolFieldView) {
                bool val = (value as JValue)?.Value<bool>() == true;
                boolFieldView.LinkToModel(val, newB => {
                    SetNewJValueInModel(self, root, new JValue(newB));
                });
                return true;
            }
            if (self.field.readOnly == true) {
                self.LinkToModel("" + value);
                return true;
            }
            return false;
        }

        private static JToken GetFieldJModel(this FieldView self, JObject root) {
            JToken model = self.GetJParent(root);
            if (model is JArray) { return model[int.Parse(self.fieldName)]; }
            return model?[self.fieldName];
        }

        private static void SetNewJValueInModel(this FieldView self, JObject root, JValue newJVal) {
            self.CreateJParentsIfNeeded(root);
            var parent = self.GetJParent(root);
            if (parent is JArray) {
                parent[int.Parse(self.fieldName)] = newJVal;
            } else {
                parent[self.fieldName] = newJVal;
            }
        }

        public static void ShowChildModelInNewScreen(this RecursiveFieldView self, JObject root, GameObject currentScreen) {
            self.openButton.SetOnClickAction(async delegate {
                var newScreen = await self.NewViewFromViewModel();
                var viewStack = currentScreen.GetViewStack();
                viewStack.ShowView(newScreen, currentScreen);
                var p = new JObjectPresenter(self.viewModelToView);
                p.targetView = newScreen;
                await p.LoadModelIntoView(self.GetFieldJModel(root) as JObject);
            }).LogOnError();
        }

        public static async Task LoadModelList(this ListFieldView self, JObject root, ViewModelToView vmtv) {
            JArray modelArray = self.GetFieldJModel(root) as JArray;
            AssertV2.IsNotNull(modelArray, "modelArray");
            for (int i = 0; i < modelArray.Count; i++) {
                JToken modelEntry = modelArray[i];
                ViewModel newEntryVm = GetMatchingViewModel(modelEntry, self.field.items);
                GameObject childView = await AddView(self, vmtv, i, newEntryVm);

                childView.GetComponentInChildren<FieldView>().LinkToJsonModel(root);
            }
        }

        private static async Task<GameObject> AddView(ListFieldView self, ViewModelToView vmtv, int i, ViewModel entryVm) {
            var parentView = self.mainLink.gameObject;
            var fieldName = "" + i;
            if (CanBeShownInListViewEntry(entryVm.GetJTokenType())) {
                var c = await vmtv.AddChild(parentView, await vmtv.NewListViewEntry());
                await vmtv.InitChild(c, fieldName, entryVm);
                return c;
            }
            return await vmtv.AddViewForFieldViewModel(parentView, entryVm, fieldName);
        }

        private static bool CanBeShownInListViewEntry(JTokenType jType) {
            if (jType == JTokenType.Integer) { return true; }
            if (jType == JTokenType.Float) { return true; }
            if (jType == JTokenType.String) { return true; }
            return false;
        }

        private static ViewModel GetMatchingViewModel(JToken modelEntry, List<ViewModel> viewModels) {
            foreach (var vm in viewModels) { if (vm.GetJTokenType() == modelEntry.Type) { return vm; } }
            return null;
        }

        private static void CreateJParentsIfNeeded(this FieldView self, JToken rootModel) {
            if (self.IsInChildObject()) { // Navigate down to the correct child JObject
                string[] parents = self.fullPath.Split(".");
                for (int i = 0; i < parents.Length - 1; i++) {
                    string parent = parents.ElementAt(i);
                    var child = GetChildJToken(rootModel, parent);
                    if (child == null) {
                        if (int.TryParse(parents.ElementAt(i + 1), out int nr)) {
                            rootModel[parent] = new JArray();
                        } else {
                            rootModel[parent] = new JObject();
                        }
                    }
                    rootModel = child;
                    AssertV2.NotNull(rootModel, $"rootModel (p='{parent}', child={child}");
                }
            }
        }

        public static JToken GetJParent(this FieldView self, JToken rootModel) {
            if (self.IsInChildObject()) { // Navigate down to the correct child JObject
                string[] parents = self.fullPath.Split(".");
                foreach (string parent in parents.Take(parents.Length - 1)) {
                    rootModel = GetChildJToken(rootModel, parent);
                }
            }
            return rootModel;
        }

        private static JToken GetChildJToken(JToken self, string entry) {
            if (int.TryParse(entry, out int i)) { // e.g. "user.friends.2.name"
                if (self is IEnumerable<JToken> list) {
                    return list.ElementAt(i);
                } else if (self is IEnumerable<KeyValuePair<string, JToken>> dict) {
                    return dict.ElementAt(i).Value;
                } else {
                    throw new NotImplementedException($"Could not get elem at pos {i} from {self}");
                }
            } else {
                return self[entry];
            }
        }
    }

}
