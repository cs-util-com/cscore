using System;
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
            await InitChild(rootView, null, null);
            await ToView(rootViewModel, await SelectInnerViewContainerFromObjectFieldView(rootView));
            return rootView;
        }

        public async Task ToView(ViewModel viewModel, V parentView) {
            foreach (var fieldName in viewModel.order) {
                ViewModel.Field field = viewModel.properties[fieldName];
                JTokenType type = field.GetJTokenType();
                if (type == JTokenType.Boolean) {
                    await InitChild(await AddChild(parentView, await NewBoolFieldView(field)), fieldName, field);
                }
                if (type == JTokenType.Integer) {
                    if (!field.contentEnum.IsNullOrEmpty()) {
                        await InitChild(await AddChild(parentView, await NewEnumFieldView(field)), fieldName, field);
                    } else {
                        await InitChild(await AddChild(parentView, await NewIntegerFieldView(field)), fieldName, field);
                    }
                }
                if (type == JTokenType.Float) {
                    await InitChild(await AddChild(parentView, await NewFloatFieldView(field)), fieldName, field);
                }
                if (type == JTokenType.String) {
                    if (!field.contentEnum.IsNullOrEmpty()) {
                        await InitChild(await AddChild(parentView, await NewEnumFieldView(field)), fieldName, field);
                    } else {
                        await InitChild(await AddChild(parentView, await NewStringFieldView(field)), fieldName, field);
                    }
                }
                if (type == JTokenType.Object) {
                    if (field.objVm.properties == null) {
                        await HandleRecursiveViewModel(parentView, fieldName, field, mtvm.viewModels.GetValue(field.objVm.modelType, null));
                    } else {
                        var objectFieldView = await NewObjectFieldView(field);
                        await InitChild(await AddChild(parentView, objectFieldView), fieldName, field);
                        await ToView(field.objVm, await SelectInnerViewContainerFromObjectFieldView(objectFieldView));
                    }
                }
                if (type == JTokenType.Array) {
                    var e = field.items;
                    if (e.Count == 1) {
                        ViewModel item = e.First();
                        var ct = EnumUtil.Parse<JTokenType>(item.type);
                        if (mtvm.IsSimpleType(ct)) {
                            await HandleSimpleArray(parentView, fieldName, field, ct);
                        } else if (ct == JTokenType.Object) {
                            await HandleObjectArray(parentView, fieldName, field, item);
                        } else {
                            throw new NotImplementedException("Array handling not impl. for type " + ct);
                        }
                    } else {
                        await HandleMixedObjectArray(parentView, fieldName, field);
                    }
                }
            }
        }

        public abstract Task<V> AddChild(V parentView, V child);
        public abstract Task InitChild(V view, string fieldName, ViewModel.Field field);

        public abstract Task<V> NewRootContainerView(ViewModel rootViewModel);

        public abstract Task<V> SelectInnerViewContainerFromObjectFieldView(V objectFieldView);

        public abstract Task<V> NewBoolFieldView(ViewModel.Field field);
        public abstract Task<V> NewStringFieldView(ViewModel.Field field);
        public abstract Task<V> NewFloatFieldView(ViewModel.Field field);
        public abstract Task<V> NewIntegerFieldView(ViewModel.Field field);
        public abstract Task<V> NewEnumFieldView(ViewModel.Field field);
        public abstract Task<V> NewObjectFieldView(ViewModel.Field field);

        public abstract Task HandleRecursiveViewModel(V parentView, string fieldName, ViewModel.Field field, ViewModel recursiveViewModel);
        public abstract Task HandleSimpleArray(V parentView, string fieldName, ViewModel.Field field, JTokenType arrayType);
        public abstract Task HandleObjectArray(V parentView, string fieldName, ViewModel.Field field, ViewModel entryViewModel);
        public abstract Task HandleMixedObjectArray(V parentView, string fieldName, ViewModel.Field field);

    }

}