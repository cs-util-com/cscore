using System;
using System.Threading.Tasks;
using com.csutil.model.jsonschema;
using UnityEngine;

namespace com.csutil.ui.jsonschema {

    /// <summary> A view generator for Unity that takes an input json schema and generates a 
    /// Unity UI from it using prefabs. This can be done on runtime or during editor time </summary>
    public class JsonSchemaToView : BaseJsonSchemaToView<GameObject> {

        public const string CONTAINER1 = "RootContainer";
        public const string CONTAINER2 = "RootContainer2";
        public const string CONTAINER3 = "RootContainer3";

        /// <summary> The folder path where all the view generation related prefabs are located, e.g. "jsonSchemaViewsV1/" </summary>
        public string prefabFolder = "jsonSchemaViewsV1/";
        public string rootContainerPrefab = CONTAINER2;
        public string objectFieldPrefab = "ObjectField";
        public string boolFieldPrefab = "BoolField";
        public string integerFieldPrefab = "IntegerField";
        public string floatFieldPrefab = "FloatField";
        public string sliderFieldPrefab = "SliderField";
        public string progressFieldPrefab = "ProgressField";
        public string stringFieldPrefab = "StringField";
        public string enumFieldPrefab = "EnumField";
        public string readOnlyTextFieldPrefab = "ReadOnlyTextField";
        public string recursiveSchemaPrefab = "RecursiveJsonSchemaUi";

        public string listFieldPrefab = "ListField";
        public string listViewEntryPrefab = "ListFieldEntry";

        /// <summary> If set to true the prefab reference will be kept, important when 
        /// generating UIs in editor time (and not dynamically during runtime) </summary>
        public bool keepReferenceToEditorPrefab = false;

        public static JsonSchemaToView NewViewGenerator() { return new JsonSchemaToView(new ModelToJsonSchema()); }

        /// <summary> Creates a generator instance that can generate views from view models vie JsonSchemaToView.ToView(..) </summary>
        public JsonSchemaToView(ModelToJsonSchema schemaGenerator) : base(schemaGenerator) { }

        public override async Task<GameObject> NewRootContainerView() { return await NewRootContainerView(rootContainerPrefab); }

        public async Task<GameObject> NewRootContainerView(string rootPrefabName) { return await LoadFieldViewPrefab(rootPrefabName); }

        public override Task<GameObject> AddChild(GameObject parent, GameObject child) {
            parent.AddChild(child);
            return Task.FromResult(child);
        }

        public override async Task<GameObject> NewObjectFieldView(JsonSchema field) { return await LoadFieldViewPrefab(objectFieldPrefab); }

        public override Task<GameObject> SelectInnerViewContainerFromObjectFieldView(GameObject containerView) {
            return Task.FromResult(containerView.GetComponentInChildren<FieldView>().mainLink.gameObject);
        }

        public override async Task<GameObject> NewBoolFieldView(JsonSchema field) { return await LoadFieldViewPrefab(boolFieldPrefab); }

        public override async Task<GameObject> NewIntegerFieldView(JsonSchema field) {
            if (field.minimum != null && field.maximum != null) {
                if (field.readOnly == true) { return await LoadFieldViewPrefab(progressFieldPrefab); }
                return await LoadFieldViewPrefab(sliderFieldPrefab);
            }
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(integerFieldPrefab);
        }

        public override async Task<GameObject> NewFloatFieldView(JsonSchema field) {
            if (field.minimum != null && field.maximum != null) {
                if (field.readOnly == true) { return await LoadFieldViewPrefab(progressFieldPrefab); }
                return await LoadFieldViewPrefab(sliderFieldPrefab);
            }
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(floatFieldPrefab);
        }

        public override async Task<GameObject> NewStringFieldView(JsonSchema field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(stringFieldPrefab);
        }

        public override async Task<GameObject> NewEnumFieldView(JsonSchema field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(enumFieldPrefab);
        }

        public override Task<GameObject> NewRecursiveSchemaView(JsonSchema field) { return LoadFieldViewPrefab(recursiveSchemaPrefab); }

        public override Task<GameObject> NewListFieldView(JsonSchema field) { return LoadFieldViewPrefab(listFieldPrefab); }

        internal Task<GameObject> NewListViewEntry() { return LoadFieldViewPrefab(listViewEntryPrefab); }

        public virtual Task<GameObject> LoadFieldViewPrefab(string prefabName) {
            return Task.FromResult(ResourcesV2.LoadPrefab(prefabFolder + prefabName, keepReferenceToEditorPrefab));
        }

        public override async Task InitChild(GameObject view, string fieldName, JsonSchema field) {
            var fieldView = view.GetComponentInChildren<FieldView>();
            fieldView.field = field;
            fieldView.fieldName = fieldName;
            var parent = view.GetParent()?.GetComponentInParents<FieldView>();
            var fullPath = fieldName;
            if (parent != null && !parent.fullPath.IsNullOrEmpty()) { fullPath = parent.fullPath + "." + fieldName; }
            view.name = fullPath;
            fieldView.fullPath = fullPath;
            await fieldView.OnViewCreated(fieldName, fullPath);
        }

        public override Task<GameObject> HandleMixedObjectArray(GameObject parent, string fieldName, JsonSchema field) {
            throw new NotImplementedException();
        }

    }

}