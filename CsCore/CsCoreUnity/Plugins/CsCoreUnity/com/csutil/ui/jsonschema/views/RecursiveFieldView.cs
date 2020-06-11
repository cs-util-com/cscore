using System.Threading.Tasks;
using com.csutil.model.jsonschema;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class RecursiveFieldView : FieldView {

        public Button openButton;
        public string rootPrefabName = JsonSchemaToView.CONTAINER3;

        public async Task<GameObject> NewViewFromSchema(JsonSchemaToView generator) {
            return await NewViewFromSchema(field, generator);
        }

        public async Task<GameObject> NewViewFromSchema(JsonSchema schema, JsonSchemaToView generator) {
            AssertV2.IsNotNull(schema, "schema");
            AssertV2.IsNotNull(generator, "generator");
            if (schema.properties == null) {
                AssertV2.IsFalse(generator.schemaGenerator.schemas.IsNullOrEmpty(), "generator.schema dict is emtpy!");
                if (generator.schemaGenerator.schemas.TryGetValue(schema.modelType, out JsonSchema vm)) {
                    schema = vm;
                } else {
                    Log.e($"No Schema found for schema.modelType={schema.modelType}");
                }
            }
            AssertV2.IsNotNull(schema.properties, "schema.properties");
            GameObject rootContainerView = await generator.NewRootContainerView(rootPrefabName);
            rootContainerView.GetComponentInChildren<FieldView>().field = schema;
            var innerContainer = await generator.SelectInnerViewContainerFromObjectFieldView(rootContainerView);
            await generator.ObjectJsonSchemaToView(schema, innerContainer);
            return rootContainerView;
        }
    }

}