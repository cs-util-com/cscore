using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class RecursiveFieldView : FieldView {

        public Button openButton;
        public string rootPrefabName = ViewModelToView.CONTAINER3;

        public async Task<GameObject> NewViewFromViewModel(ViewModelToView viewModelToView) {
            return await NewViewFromViewModel(field, viewModelToView);
        }

        public async Task<GameObject> NewViewFromViewModel(JsonSchema viewModel, ViewModelToView viewModelToView) {
            AssertV2.IsNotNull(viewModel, "viewModel");
            AssertV2.IsNotNull(viewModelToView, "viewModelToView");
            if (viewModel.properties == null) {
                AssertV2.IsFalse(viewModelToView.mtvm.viewModels.IsNullOrEmpty(), "viewModels map is emtpy!");
                if (viewModelToView.mtvm.viewModels.TryGetValue(viewModel.modelType, out JsonSchema vm)) {
                    viewModel = vm;
                } else {
                    Log.e($"No ViewModel found for viewModel.modelType={viewModel.modelType}");
                }
            }
            AssertV2.IsNotNull(viewModel.properties, "viewModel.fields");
            GameObject rootContainerView = await viewModelToView.NewRootContainerView(rootPrefabName);
            rootContainerView.GetComponentInChildren<FieldView>().field = viewModel;
            var innerContainer = await viewModelToView.SelectInnerViewContainerFromObjectFieldView(rootContainerView);
            await viewModelToView.ObjectViewModelToView(viewModel, innerContainer);
            return rootContainerView;
        }
    }

}