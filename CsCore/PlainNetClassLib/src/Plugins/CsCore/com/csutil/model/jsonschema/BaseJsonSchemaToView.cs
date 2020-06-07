using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace com.csutil.model.jsonschema {

    /// <summary> An abstract generator that can create a view from an input json schema </summary>
    /// <typeparam name="V">The view type, in Unity views for example are made out of GameObjects </typeparam>
    public abstract class BaseJsonSchemaToView<V> {

        public ModelToJsonSchema schemaGenerator;

        public BaseJsonSchemaToView(ModelToJsonSchema schemaGenerator) {
            this.schemaGenerator = schemaGenerator;
        }

        public async Task<V> ToView(JsonSchema rootSchema) {
            var rootView = await NewRootContainerView();
            await InitChild(rootView, null, rootSchema);
            await ObjectJsonSchemaToView(rootSchema, await SelectInnerViewContainerFromObjectFieldView(rootView));
            return rootView;
        }

        public async Task ObjectJsonSchemaToView(JsonSchema schema, V parentView) {
            foreach (var fieldName in schema.GetOrder()) {
                JsonSchema field = schema.properties[fieldName];
                await AddViewForJsonSchemaField(parentView, field, fieldName);
            }
        }

        public async Task<V> AddViewForJsonSchemaField(V parentView, JsonSchema field, string fieldName) {
            JTokenType type = field.GetJTokenType();
            if (type == JTokenType.Boolean) {
                return await AddAndInit(parentView, field, fieldName, await NewBoolFieldView(field));
            }
            if (type == JTokenType.Integer) {
                if (!field.contentEnum.IsNullOrEmpty()) {
                    return await AddAndInit(parentView, field, fieldName, await NewEnumFieldView(field));
                } else {
                    return await AddAndInit(parentView, field, fieldName, await NewIntegerFieldView(field));
                }
            }
            if (type == JTokenType.Float) {
                return await AddAndInit(parentView, field, fieldName, await NewFloatFieldView(field));
            }
            if (type == JTokenType.String) {
                if (!field.contentEnum.IsNullOrEmpty()) {
                    return await AddAndInit(parentView, field, fieldName, await NewEnumFieldView(field));
                } else {
                    return await AddAndInit(parentView, field, fieldName, await NewStringFieldView(field));
                }
            }
            if (type == JTokenType.Object) {
                if (field.properties == null) {
                    return await AddAndInit(parentView, field, fieldName, await NewRecursiveSchemaView(field));
                } else {
                    var objectFieldView = await NewObjectFieldView(field);
                    await InitChild(await AddChild(parentView, objectFieldView), fieldName, field);
                    await ObjectJsonSchemaToView(field, await SelectInnerViewContainerFromObjectFieldView(objectFieldView));
                    return objectFieldView;
                }
            }
            if (type == JTokenType.Array) {
                var e = field.items;
                if (e.Count == 1) {
                    JsonSchema item = e.First();
                    var childJType = item.GetJTokenType();
                    if (schemaGenerator.IsSimpleType(childJType)) {
                        return await AddAndInit(parentView, field, fieldName, await NewListFieldView(field));
                    } else if (childJType == JTokenType.Object) {
                        return await AddAndInit(parentView, field, fieldName, await NewListFieldView(field));
                    } else {
                        throw new NotImplementedException("Array handling not impl. for type " + item.type);
                    }
                } else {
                    return await HandleMixedObjectArray(parentView, fieldName, field);
                }
            }
            throw new NotImplementedException($"Did not handle field {field.title} of type={type}");
        }


        private async Task<V> AddAndInit(V parentView, JsonSchema field, string fieldName, V newChildView) {
            var c = await AddChild(parentView, newChildView);
            await InitChild(c, fieldName, field);
            return c;
        }

        public abstract Task<V> AddChild(V parentView, V child);
        public abstract Task InitChild(V view, string fieldName, JsonSchema field);

        public abstract Task<V> NewRootContainerView();

        public abstract Task<V> SelectInnerViewContainerFromObjectFieldView(V objectFieldView);

        public abstract Task<V> NewBoolFieldView(JsonSchema field);
        public abstract Task<V> NewStringFieldView(JsonSchema field);
        public abstract Task<V> NewFloatFieldView(JsonSchema field);
        public abstract Task<V> NewIntegerFieldView(JsonSchema field);
        public abstract Task<V> NewEnumFieldView(JsonSchema field);
        public abstract Task<V> NewObjectFieldView(JsonSchema field);

        public abstract Task<V> NewRecursiveSchemaView(JsonSchema field);
        public abstract Task<V> NewListFieldView(JsonSchema field);
        public abstract Task<V> HandleMixedObjectArray(V parentView, string fieldName, JsonSchema field);

    }

}