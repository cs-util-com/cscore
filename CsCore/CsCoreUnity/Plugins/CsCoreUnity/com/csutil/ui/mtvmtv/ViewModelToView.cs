using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using Newtonsoft.Json.Linq;
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
        public string stringFieldPrefab = "StringField";
        public string enumFieldPrefab = "EnumField";
        public string readOnlyTextFieldPrefab = "ReadOnlyTextField";
        public string recursiveViewModelPrefab = "RecursiveViewModelUi";

        public ViewModelToView(ModelToViewModel mtvm, string prefabFolder = "mtvmtv1/") : base(mtvm) {
            this.prefabFolder = prefabFolder;
        }

        public override async Task<GameObject> NewRootContainerView(ViewModel rootViewModel) {
            return await NewRootContainerView(rootViewModel, rootContainerPrefab);
        }

        public async Task<GameObject> NewRootContainerView(ViewModel rootViewModel, string rootPrefabName) {
            var name = rootViewModel.modelName;
            var rootView = await LoadFieldViewPrefab(rootPrefabName);
            SetViewModel(rootView, rootViewModel);
            return rootView;
        }

        public override Task<GameObject> AddChild(GameObject parent, GameObject child) {
            parent.AddChild(child);
            return Task.FromResult(child);
        }

        public override async Task<GameObject> NewObjectFieldView(ViewModel.Field field) {
            return await LoadFieldViewPrefab(objectFieldPrefab);
        }

        public override Task<GameObject> SelectInnerViewContainerFromObjectFieldView(GameObject containerView) {
            return Task.FromResult(containerView.GetComponentInChildren<FieldView>().mainLink.gameObject);
        }

        public override async Task<GameObject> NewBoolFieldView(ViewModel.Field field) {
            return await LoadFieldViewPrefab(boolFieldPrefab);
        }

        public override async Task<GameObject> NewIntegerFieldView(ViewModel.Field field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(integerFieldPrefab);
        }

        public override async Task<GameObject> NewFloatFieldView(ViewModel.Field field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(floatFieldPrefab);
        }

        public override async Task<GameObject> NewStringFieldView(ViewModel.Field field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(stringFieldPrefab);
        }

        public override async Task<GameObject> NewEnumFieldView(ViewModel.Field field) {
            if (field.readOnly == true) { return await LoadFieldViewPrefab(readOnlyTextFieldPrefab); }
            return await LoadFieldViewPrefab(enumFieldPrefab);
        }

        public virtual Task<GameObject> LoadFieldViewPrefab(string prefabName) {
            return Task.FromResult(ResourcesV2.LoadPrefab(prefabFolder + prefabName));
        }

        public override async Task InitChild(GameObject view, string fieldName, ViewModel.Field field) {
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

        public override async Task HandleRecursiveViewModel(GameObject parent, string fieldName, ViewModel.Field field, ViewModel recursiveViewModel) {
            var view = await AddChild(parent, await LoadFieldViewPrefab(recursiveViewModelPrefab));
            SetViewModel(view, recursiveViewModel);
            await InitChild(view, fieldName, field);
        }

        private void SetViewModel(GameObject view, ViewModel viewModel) {
            var viewModelFieldView = view.GetComponentInChildren<ViewModelFieldView>();
            viewModelFieldView.viewModelToView = this;
            viewModelFieldView.recursiveViewModel = viewModel;
        }

        public override Task HandleSimpleArray(GameObject parent, string fieldName, ViewModel.Field field, JTokenType arrayType) {
            throw new NotImplementedException();
        }

        public override Task HandleMixedObjectArray(GameObject parent, string fieldName, ViewModel.Field field) {
            throw new NotImplementedException();
        }

        public override Task HandleObjectArray(GameObject parent, string fieldName, ViewModel.Field field, ViewModel entryViewModel) {
            throw new NotImplementedException();
        }

    }

}