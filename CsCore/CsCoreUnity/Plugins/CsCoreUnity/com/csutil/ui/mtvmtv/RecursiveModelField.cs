﻿using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class RecursiveModelField : ViewModelFieldView {

        public Button openButton;
        public string rootPrefabName = ViewModelToView.CONTAINER3;

        public async Task<GameObject> NewViewFromViewModel() {
            return await NewViewFromViewModel(recursiveViewModel != null ? recursiveViewModel : field);
        }

        public async Task<GameObject> NewViewFromViewModel(ViewModel viewModel) {
            AssertV2.NotNull(viewModel, "viewModel");
            AssertV2.NotNull(viewModel.properties, "viewModel.fields");
            GameObject rootContainerView = await viewModelToView.NewRootContainerView(viewModel, rootPrefabName);
            var innerContainer = await viewModelToView.SelectInnerViewContainerFromObjectFieldView(rootContainerView);
            await viewModelToView.ToView(viewModel, innerContainer);
            return rootContainerView;
        }
    }

}