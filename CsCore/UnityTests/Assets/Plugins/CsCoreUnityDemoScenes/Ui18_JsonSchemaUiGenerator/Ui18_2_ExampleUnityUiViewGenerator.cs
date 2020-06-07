﻿using com.csutil.model.mtvmtv;
using com.csutil.ui.mtvmtv;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests {

    class Ui18_2_ExampleUnityUiViewGenerator : MonoBehaviour {

        public string prefabFolder = "mtvmtv1/";
        public string containerPrefabToUse = JsonSchemaToView.CONTAINER2;

        /// <summary> Has to be triggered by the develper via the Unity editor UI to start the 
        /// view generation. See the infos in GenerateViewFromClass() below </summary>
        public bool GenerateUiNow;

        private void OnEnable() { ShowModelInstanceInView().LogOnError(); }

        private void OnValidate() {
            if (GenerateUiNow) {
                GenerateUiNow = false;
                GenerateViewFromClass<Ui18_1_JsonSchemaUiGenerator.MyUserModel>().LogOnError();
            }
        }

        /// <summary>
        /// Generates a new view from a target model T and adds it as a child gameobject so 
        /// that the developer can start modifying it afterwards. This should typically be 
        /// triggered in Editor time, but if no modification of the UI is needed it could also
        /// be done on runtime. The generated view can be saved as a prefab and loaded later during
        /// runtime when the actual instance of the model T should be shown in it. 
        /// </summary>
        private async Task GenerateViewFromClass<T>() {
            GameObject generatedView = await NewViewModelToView().GenerateViewFrom<T>(true);
            AssertV2.IsTrue(gameObject.GetChildCount() == 0, "Please delete previous generated views");
            gameObject.AddChild(generatedView);
        }

        private async Task ShowModelInstanceInView() {
            // Get the previously created view (see above)
            var uiView = gameObject.GetChild(0);
            AssertV2.IsNotNull(uiView, "uiView");
            // Create some example model instance:
            var modelInstance = Ui18_1_JsonSchemaUiGenerator.NewExampleUserInstance();

            JObjectPresenter p = new JObjectPresenter(NewViewModelToView());
            p.targetView = uiView;
            var changedInstance = await p.LoadViaJsonIntoView(modelInstance, VmtvContainerUtil.ChangesSavedViaConfirmButton(uiView));
            uiView.Destroy(); // "Close" the view after the user clicked confirm

            var changedFields = MergeJson.GetDiff(modelInstance, changedInstance);
            Log.d("Fields changed: " + changedFields?.ToPrettyString());
        }

        private JsonSchemaToView NewViewModelToView() {
            return new JsonSchemaToView(new ModelToJsonSchema(), prefabFolder) { rootContainerPrefab = containerPrefabToUse };
        }

    }

}