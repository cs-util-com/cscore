using System;
using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine;

namespace com.csutil.ui.mtvmtv {

    public class ViewModelToView : BaseViewModelToView<GameObject> {

        public const string CONTAINER1 = "RootContainer";
        public const string CONTAINER2 = "RootContainer2";
        public const string CONTAINER3 = "RootContainer3";

        public string prefabFolder;
        public string rootContainerPrefab = CONTAINER1;
        public string objectFieldPrefab = "ObjectField";
        public string boolFieldPrefab = "BoolField";
        public string integerFieldPrefab = "IntegerField";
        public string floatFieldPrefab = "FloatField";
        public string sliderFieldPrefab = "SliderField";
        public string stringFieldPrefab = "StringField";
        public string enumFieldPrefab = "EnumField";
        public string readOnlyTextFieldPrefab = "ReadOnlyTextField";
        public string recursiveViewModelPrefab = "RecursiveViewModelUi";

        public string listFieldPrefab = "ListField";
        public string listViewEntryPrefab = "ListFieldEntry";

        /// <summary> If set to true the prefab reference will be kept, important when 
        /// generating UIs in editor time (and not dynamically during runtime) </summary>
        public bool keepReferenceToEditorPrefab = false;

        /// <summary> Creates a generator instance that can generate views from view models vie ViewModelToView.ToView(..) </summary>
        /// <param name="prefabFolder"> The folder path where all the view generation related prefabs are located, e.g. "mtvmtv1/" </param>
        public ViewModelToView(ModelToViewModel mtvm, string prefabFolder) : base(mtvm) {
            this.prefabFolder = prefabFolder;
        }

        public override async Task<GameObject> NewRootContainerView() {
            return await NewRootContainerView(rootContainerPrefab);
        }

        public async Task<GameObject> NewRootContainerView(string rootPrefabName) {
            return await LoadFieldViewPrefab(rootPrefabName);
        }

        public override Task<GameObject> AddChild(GameObject parent, GameObject child) {
            parent.AddChild(child);
            return Task.FromResult(child);
        }

        public override async Task<GameObject> NewObjectFieldView(ViewModel field) {
            return await LoadFieldViewPrefab(objectFieldPrefab);
        }

        public override Task<GameObject> SelectInnerViewContainerFromObjectFieldView(GameObject containerView) {
            return Task.FromResult(containerView.GetComponentInChildren<FieldView>().mainLink.gameObject);
        }

        public override async Task<GameObject> NewBoolFieldView(ViewModel field) {
            return await LoadFieldViewPrefab(boolFieldPrefab);
        }

        public override async Task<GameObject> NewIntegerFieldView(ViewModel field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            if (field.minimum != null && field.maximum != null) { return await LoadFieldViewPrefab(sliderFieldPrefab); }
            return await LoadFieldViewPrefab(integerFieldPrefab);
        }

        public override async Task<GameObject> NewFloatFieldView(ViewModel field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            if (field.minimum != null && field.maximum != null) { return await LoadFieldViewPrefab(sliderFieldPrefab); }
            return await LoadFieldViewPrefab(floatFieldPrefab);
        }

        public override async Task<GameObject> NewStringFieldView(ViewModel field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(stringFieldPrefab);
        }

        public override async Task<GameObject> NewEnumFieldView(ViewModel field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(enumFieldPrefab);
        }

        internal Task<GameObject> NewListViewEntry() {
            return LoadFieldViewPrefab(listViewEntryPrefab);
        }

        public virtual Task<GameObject> LoadFieldViewPrefab(string prefabName) {
            return Task.FromResult(ResourcesV2.LoadPrefab(prefabFolder + prefabName, keepReferenceToEditorPrefab));
        }

        public override async Task InitChild(GameObject view, string fieldName, ViewModel field) {
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

        public override async Task<GameObject> HandleRecursiveViewModel(GameObject parent, string fieldName, ViewModel field, ViewModel recursiveViewModel) {
            var view = await AddChild(parent, await LoadFieldViewPrefab(recursiveViewModelPrefab));
            await InitChild(view, fieldName, field);
            return view;
        }

        public override async Task<GameObject> HandleSimpleArray(GameObject parent, string fieldName, ViewModel field) {
            var view = await AddChild(parent, await LoadFieldViewPrefab(listFieldPrefab));
            await InitChild(view, fieldName, field);
            return view;
        }

        public override async Task<GameObject> HandleObjectArray(GameObject parent, string fieldName, ViewModel field) {
            var view = await AddChild(parent, await LoadFieldViewPrefab(listFieldPrefab));
            await InitChild(view, fieldName, field);
            return view;
        }

        public override Task<GameObject> HandleMixedObjectArray(GameObject parent, string fieldName, ViewModel field) {
            throw new NotImplementedException();
        }

    }

}