using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace com.csutil.ui.mtvmtv {

    public class ViewModelToView : BaseViewModelToView<GameObject> {

        public string prefabFolder;
        public string rootContainerPrefab = "RootContainer";
        public string objectFieldPrefab = "ObjectField";
        public string boolFieldPrefab = "BoolField";
        public string integerFieldPrefab = "IntegerField";
        public string floatFieldPrefab = "FloatField";
        public string stringFieldPrefab = "StringField";
        public string readOnlyTextFieldPrefab = "ReadOnlyTextField";
        public string recursiveViewModelPrefab = "RecursiveViewModelUi";

        public ViewModelToView(ModelToViewModel mtvm, string prefabFolder = "mtvmtv1/") : base(mtvm) {
            this.prefabFolder = prefabFolder;
        }

        public override async Task<GameObject> NewRootContainerView(ViewModel rootViewModel) {
            var name = rootViewModel.modelName;
            return await OnFieldViewCreated(await LoadFieldView(rootContainerPrefab, name), name, null);
        }

        public override Task AddChildView(GameObject parent, GameObject child) {
            parent.AddChild(child);
            return Task.FromResult(true);
        }

        public override async Task<GameObject> NewObjectFieldView(string fieldName, ViewModel.Field field) {
            return await OnFieldViewCreated(await LoadFieldView(objectFieldPrefab, fieldName), fieldName, field);
        }

        public override Task<GameObject> SelectInnerViewContainerFromObjectFieldView(GameObject containerView) {
            return Task.FromResult(containerView.GetComponentInChildren<FieldView>().mainLink.gameObject);
        }

        public override async Task<GameObject> NewBoolFieldView(string fieldName, ViewModel.Field field) {
            return await OnFieldViewCreated(await LoadFieldView(boolFieldPrefab, fieldName), fieldName, field);
        }

        public override async Task<GameObject> NewIntegerFieldView(string fieldName, ViewModel.Field field) {
            if (field.readOnly == true) {
                return await OnFieldViewCreated(await LoadFieldView(readOnlyTextFieldPrefab, fieldName), fieldName, field);
            }
            return await OnFieldViewCreated(await LoadFieldView(integerFieldPrefab, fieldName), fieldName, field);
        }

        public override async Task<GameObject> NewFloatFieldView(string fieldName, ViewModel.Field field) {
            if (field.readOnly == true) {
                return await OnFieldViewCreated(await LoadFieldView(readOnlyTextFieldPrefab, fieldName), fieldName, field);
            }
            return await OnFieldViewCreated(await LoadFieldView(floatFieldPrefab, fieldName), fieldName, field);
        }

        public override async Task<GameObject> NewStringFieldView(string fieldName, ViewModel.Field field) {
            if (field.readOnly == true) {
                return await OnFieldViewCreated(await LoadFieldView(readOnlyTextFieldPrefab, fieldName), fieldName, field);
            }
            return await OnFieldViewCreated(await LoadFieldView(stringFieldPrefab, fieldName), fieldName, field);
        }

        public virtual Task<GameObject> LoadFieldView(string prefabName, string goName) {
            var loadedPrefabGo = ResourcesV2.LoadPrefab(prefabFolder + prefabName);
            loadedPrefabGo.name = goName;
            return Task.FromResult(loadedPrefabGo);
        }

        public virtual async Task<GameObject> OnFieldViewCreated(GameObject view, string fieldName, ViewModel.Field field) {
            var fieldView = view.GetComponentInChildren<FieldView>();
            fieldView.field = field;
            await fieldView.OnViewCreated(fieldName);
            return view;
        }

        public override async Task HandleRecursiveViewModel(GameObject parent, string fieldName, ViewModel.Field field, ViewModel recursiveViewModel) {
            var view = await LoadFieldView(recursiveViewModelPrefab, fieldName);
            await OnFieldViewCreated(view, fieldName, field);
            var rmf = view.GetComponentInChildren<RecursiveModelField>();
            rmf.viewModelToView = this;
            rmf.recursiveViewModel = recursiveViewModel;
            await AddChildView(parent, view);
        }

        public override Task HandleSimpleArray(GameObject parent, string fieldName, ViewModel.Field field, JTokenType arrayType) {
            throw new System.NotImplementedException();
        }

        public override Task HandleMixedObjectArray(GameObject parent, string fieldName, ViewModel.Field field) {
            throw new System.NotImplementedException();
        }

        public override Task HandleObjectArray(GameObject parent, string fieldName, ViewModel.Field field, ViewModel entryViewModel) {
            throw new System.NotImplementedException();
        }

    }

}