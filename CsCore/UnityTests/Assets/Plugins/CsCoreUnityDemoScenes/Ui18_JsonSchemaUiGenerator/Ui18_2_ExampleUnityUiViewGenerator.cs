using com.csutil.model.jsonschema;
using com.csutil.ui.jsonschema;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests.jsonschema {

    class Ui18_2_ExampleUnityUiViewGenerator : MonoBehaviour {

        public string prefabFolder = "jsonSchemaViewsV1/";
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
            GameObject generatedView = await NewViewGenerator().GenerateViewFrom<T>(true);

            gameObject.AddChild(generatedView);
            if (gameObject.GetChildCount() != 1) {
                var viewToUpdate = gameObject.GetChildren().First();
                FieldViewUpdater.UpdateFieldViews(viewToUpdate, generatedView);
            }
        }

        private async Task ShowModelInstanceInView() {
            // Get the previously created view (see above)
            var uiView = gameObject.GetChild(0);
            AssertV2.IsNotNull(uiView, "uiView");
            // Create some example model instance:
            var modelInstance = Ui18_1_JsonSchemaUiGenerator.NewExampleUserInstance();

            JsonSchemaPresenter p = new JsonSchemaPresenter(NewViewGenerator());
            p.targetView = uiView;
            var changedInstance = await p.LoadViaJsonIntoView(modelInstance);
            uiView.Destroy(); // Close the view by destroying it after the user done with it

            var changedFields = MergeJson.GetDiff(modelInstance, changedInstance);
            Log.d("Fields changed: " + changedFields?.ToPrettyString());
        }

        private JsonSchemaToView NewViewGenerator() {
            return new JsonSchemaToView(new ModelToJsonSchema(), prefabFolder) { rootContainerPrefab = containerPrefabToUse };
        }

    }

}
