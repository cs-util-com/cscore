using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace com.csutil.model.mtvmtv {

    /// <summary> An abstract generator that can create a view from an input json schema </summary>
    /// <typeparam name="V">The view type, in Unity views for example are made out of GameObjects </typeparam>
    public abstract class BaseJsonSchemaToView<V> {

        public ModelToJsonSchema mtvm;

        public BaseJsonSchemaToView(ModelToJsonSchema mtvm) {
            this.mtvm = mtvm;
        }

        public async Task<V> ToView(JsonSchema rootViewModel) {
            var rootView = await NewRootContainerView();
            await InitChild(rootView, null, rootViewModel);
            await ObjectViewModelToView(rootViewModel, await SelectInnerViewContainerFromObjectFieldView(rootView));
            return rootView;
        }

        public async Task ObjectViewModelToView(JsonSchema viewModel, V parentView) {
            foreach (var fieldName in viewModel.GetOrder()) {
                JsonSchema field = viewModel.properties[fieldName];
                await AddViewForFieldViewModel(parentView, field, fieldName);
            }
        }

        public async Task<V> AddViewForFieldViewModel(V parentView, JsonSchema field, string fieldName) {
            JTokenType type = field.GetJTokenType();
            if (type == JTokenType.Boolean) {
                var c = await AddChild(parentView, await NewBoolFieldView(field));
                await InitChild(c, fieldName, field);
                return c;
            }
            if (type == JTokenType.Integer) {
                if (!field.contentEnum.IsNullOrEmpty()) {
                    var c = await AddChild(parentView, await NewEnumFieldView(field));
                    await InitChild(c, fieldName, field);
                    return c;

                } else {
                    var c = await AddChild(parentView, await NewIntegerFieldView(field));
                    await InitChild(c, fieldName, field);
                    return c;
                }
            }
            if (type == JTokenType.Float) {
                var c = await AddChild(parentView, await NewFloatFieldView(field));
                await InitChild(c, fieldName, field);
                return c;
            }
            if (type == JTokenType.String) {
                if (!field.contentEnum.IsNullOrEmpty()) {
                    var c = await AddChild(parentView, await NewEnumFieldView(field));
                    await InitChild(c, fieldName, field);
                    return c;
                } else {
                    var c = await AddChild(parentView, await NewStringFieldView(field));
                    await InitChild(c, fieldName, field);
                    return c;
                }
            }
            if (type == JTokenType.Object) {
                if (field.properties == null) {
                    return await HandleRecursiveViewModel(parentView, fieldName, field, mtvm.viewModels.GetValue(field.modelType, null));
                } else {
                    var objectFieldView = await NewObjectFieldView(field);
                    await InitChild(await AddChild(parentView, objectFieldView), fieldName, field);
                    await ObjectViewModelToView(field, await SelectInnerViewContainerFromObjectFieldView(objectFieldView));
                    return objectFieldView;
                }
            }
            if (type == JTokenType.Array) {
                var e = field.items;
                if (e.Count == 1) {
                    JsonSchema item = e.First();
                    var childJType = item.GetJTokenType();
                    if (mtvm.IsSimpleType(childJType)) {
                        return await HandleSimpleArray(parentView, fieldName, field);
                    } else if (childJType == JTokenType.Object) {
                        return await HandleObjectArray(parentView, fieldName, field);
                    } else {
                        throw new NotImplementedException("Array handling not impl. for type " + item.type);
                    }
                } else {
                    return await HandleMixedObjectArray(parentView, fieldName, field);
                }
            }
            throw new NotImplementedException($"Did not handle field {field.title} of type={type}");
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

        public abstract Task<V> HandleRecursiveViewModel(V parentView, string fieldName, JsonSchema field, JsonSchema recursiveViewModel);
        public abstract Task<V> HandleSimpleArray(V parentView, string fieldName, JsonSchema field);
        public abstract Task<V> HandleObjectArray(V parentView, string fieldName, JsonSchema field);
        public abstract Task<V> HandleMixedObjectArray(V parentView, string fieldName, JsonSchema field);

    }

}