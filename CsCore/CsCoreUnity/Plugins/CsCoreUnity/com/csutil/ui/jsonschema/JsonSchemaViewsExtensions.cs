using com.csutil.datastructures;
using com.csutil.model.jsonschema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    /// <summary> Methods that help setting up views that where generated via a Json schema </summary>
    public static class JsonSchemaViewsExtensions {

        /// <summary> Can be used to generate a view directly from a model, if the json schema does not have to be customized, e.g. 
        /// because the model uses Annotations this is the easiest way to generate a fully usable UI from any class </summary>
        /// <typeparam name="T"> The type of the model </typeparam>
        /// <param name="keepReferenceToEditorPrefab"> If the view is generated during editor time this should be set to 
        /// true so that the used prefabs in the view are still linked correctly. </param>
        /// <returns> The generated view which can be used to load a model instance into it </returns>
        public static async Task<GameObject> GenerateViewFrom<T>(this JsonSchemaToView self, bool keepReferenceToEditorPrefab = false) {
            return await self.GenerateViewFrom(typeof(T), keepReferenceToEditorPrefab);
        }

        /// <summary> Can be used to generate a view directly from a model, if the json schema does not have to be customized, e.g. 
        /// because the model uses Annotations this is the easiest way to generate a fully usable UI from any class </summary>
        /// true so that the used prefabs in the view are still linked correctly. </param>
        /// <param name="modelType"> The type of the model </param>
        /// <param name="keepReferenceToEditorPrefab"> If the view is generated during editor time this should be set to 
        /// <returns> The generated view which can be used to load a model instance into it </returns>
        public static async Task<GameObject> GenerateViewFrom(this JsonSchemaToView self, Type modelType, bool keepReferenceToEditorPrefab = false) {
            var timing = Log.MethodEnteredWith(modelType);
            JsonSchema schema = self.schemaGenerator.ToJsonSchema(modelType.Name, modelType);
            self.keepReferenceToEditorPrefab = keepReferenceToEditorPrefab;
            var view = await self.ToView(schema);
            view.name = schema.title;
            Log.MethodDone(timing);
            return view;
        }

        public static Dictionary<string, FieldView> GetFieldViewMap(this GameObject self, bool includeInactive = true) {
            return self.GetComponentsInChildren<FieldView>(includeInactive).Filter(x => !x.fullPath.IsNullOrEmpty()).ToDictionary(x => x.fullPath, x => x);
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

        public static FieldView LinkViewToModel(this Dictionary<string, FieldView> self, string key, string val, Action<string> onNewVal) {
            var fv = self.Get<FieldView>(key);
            if (fv == null) { throw new ArgumentException("Cant link view to model, key not found: " + key); }
            if (fv is InputFieldView ifv) { return ifv.LinkToModel(val, onNewVal); }
            if (fv is SliderFieldView sfv) { return sfv.LinkToModel(float.Parse(val), nv => onNewVal("" + nv)); }
            throw new NotImplementedException("Cant link view to model, unhandled field view type " + fv.GetType());
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

        public static SliderFieldView LinkToModel(this SliderFieldView self, float val, Action<float> onNewVal) {
            self.slider.value = val;
            self.valueDisplay.text = val + " / " + self.field.maximum;
            self.slider.AddOnValueChangedAction(newVal => {
                self.valueDisplay.text = newVal + " / " + self.field.maximum;
                return true;
            });
            self.slider.AddOnValueChangedActionThrottled(newVal => {
                onNewVal(newVal);
            }, 100);
            return self;
        }

        public static bool IsInChildObject(this FieldView self) { return self.fieldName != self.fullPath; }

    }

}