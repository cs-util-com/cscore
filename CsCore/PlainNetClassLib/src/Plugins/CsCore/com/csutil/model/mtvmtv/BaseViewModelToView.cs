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
                ViewModel.Field field = viewModel.fields[fieldName];
                var type = EnumUtil.Parse<JTokenType>(field.type);
                if (type == JTokenType.Boolean) {
                    await InitChild(await AddChild(parentView, await NewBoolFieldView(field)), fieldName, field);
                }
                if (type == JTokenType.Integer) {
                    await InitChild(await AddChild(parentView, await NewIntegerFieldView(field)), fieldName, field);
                }
                if (type == JTokenType.Float) {
                    await InitChild(await AddChild(parentView, await NewFloatFieldView(field)), fieldName, field);
                }
                if (type == JTokenType.String) {
                    await InitChild(await AddChild(parentView, await NewStringFieldView(field)), fieldName, field);
                }
                if (type == JTokenType.Object) {
                    if (field.objVm.fields == null) {
                        await HandleRecursiveViewModel(parentView, fieldName, field, mtvm.viewModels.GetValue(field.objVm.modelType, null));
                    } else {
                        var objectFieldView = await NewObjectFieldView(field);
                        await InitChild(await AddChild(parentView, objectFieldView), fieldName, field);
                        await ToView(field.objVm, await SelectInnerViewContainerFromObjectFieldView(objectFieldView));
                    }
                }
                if (type == JTokenType.Array) {
                    var ct = EnumUtil.Parse<JTokenType>(field.children.type);
                    if (mtvm.IsSimpleType(ct)) {
                        await HandleSimpleArray(parentView, fieldName, field, ct);
                    } else if (ct == JTokenType.Object) {
                        var e = field.children.entries;
                        if (e.Count == 1) {
                            await HandleObjectArray(parentView, fieldName, field, e.First());
                        } else {
                            await HandleMixedObjectArray(parentView, fieldName, field);
                        }
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
        public abstract Task<V> NewObjectFieldView(ViewModel.Field field);

        public abstract Task HandleRecursiveViewModel(V parentView, string fieldName, ViewModel.Field field, ViewModel recursiveViewModel);
        public abstract Task HandleSimpleArray(V parentView, string fieldName, ViewModel.Field field, JTokenType arrayType);
        public abstract Task HandleObjectArray(V parentView, string fieldName, ViewModel.Field field, ViewModel entryViewModel);
        public abstract Task HandleMixedObjectArray(V parentView, string fieldName, ViewModel.Field field);

    }

}