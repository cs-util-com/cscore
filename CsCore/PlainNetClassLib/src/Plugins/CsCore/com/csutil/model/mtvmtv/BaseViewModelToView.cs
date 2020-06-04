using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace com.csutil.model.mtvmtv {

    public abstract class BaseViewModelToView<V> {

        public ModelToViewModel mtvm;

        public BaseViewModelToView(ModelToViewModel mtvm) {
            this.mtvm = mtvm;
        }

        public async Task<V> ToView(ViewModel rootViewModel) {
            var rootView = await NewRootContainerView(rootViewModel);
            await InitChild(rootView, null, rootViewModel);
            await ObjectViewModelToView(rootViewModel, await SelectInnerViewContainerFromObjectFieldView(rootView));
            return rootView;
        }

        public async Task ObjectViewModelToView(ViewModel viewModel, V parentView) {
            foreach (var fieldName in viewModel.GetOrder()) {
                ViewModel field = viewModel.properties[fieldName];
                await AddViewForFieldViewModel(parentView, field, fieldName);
            }
        }

        public async Task<V> AddViewForFieldViewModel(V parentView, ViewModel field, string fieldName) {
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
                    ViewModel item = e.First();
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
        public abstract Task InitChild(V view, string fieldName, ViewModel field);

        public abstract Task<V> NewRootContainerView(ViewModel rootViewModel);

        public abstract Task<V> SelectInnerViewContainerFromObjectFieldView(V objectFieldView);

        public abstract Task<V> NewBoolFieldView(ViewModel field);
        public abstract Task<V> NewStringFieldView(ViewModel field);
        public abstract Task<V> NewFloatFieldView(ViewModel field);
        public abstract Task<V> NewIntegerFieldView(ViewModel field);
        public abstract Task<V> NewEnumFieldView(ViewModel field);
        public abstract Task<V> NewObjectFieldView(ViewModel field);

        public abstract Task<V> HandleRecursiveViewModel(V parentView, string fieldName, ViewModel field, ViewModel recursiveViewModel);
        public abstract Task<V> HandleSimpleArray(V parentView, string fieldName, ViewModel field);
        public abstract Task<V> HandleObjectArray(V parentView, string fieldName, ViewModel field);
        public abstract Task<V> HandleMixedObjectArray(V parentView, string fieldName, ViewModel field);

    }

}