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
            await ToView(rootViewModel, await SelectInnerViewContainerFromObjectFieldView(rootView));
            return rootView;
        }

        public async Task ToView(ViewModel viewModel, V parentView) {
            foreach (var fieldName in viewModel.order) {
                ViewModel.Field field = viewModel.fields[fieldName];
                var type = EnumUtil.Parse<JTokenType>(field.type);
                if (type == JTokenType.Boolean) {
                    await AddChildView(parentView, await NewBoolFieldView(fieldName, field));
                }
                if (type == JTokenType.Integer) {
                    await AddChildView(parentView, await NewIntegerFieldView(fieldName, field));
                }
                if (type == JTokenType.Float) {
                    await AddChildView(parentView, await NewFloatFieldView(fieldName, field));
                }
                if (type == JTokenType.String) {
                    await AddChildView(parentView, await NewStringFieldView(fieldName, field));
                }
                if (type == JTokenType.Object) {
                    if (field.objVm.fields == null) {
                        await HandleRecursiveViewModel(parentView, fieldName, field, mtvm.viewModels.GetValue(field.objVm.modelType, null));
                    } else {
                        var objectFieldView = await NewObjectFieldView(fieldName, field);
                        await AddChildView(parentView, objectFieldView);
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

        public abstract Task<V> NewRootContainerView(ViewModel rootViewModel);
        public abstract Task AddChildView(V parentView, V child);

        public abstract Task<V> NewObjectFieldView(string fieldName, ViewModel.Field field);
        public abstract Task<V> SelectInnerViewContainerFromObjectFieldView(V objectFieldView);

        public abstract Task<V> NewBoolFieldView(string fieldName, ViewModel.Field field);
        public abstract Task<V> NewStringFieldView(string fieldName, ViewModel.Field field);
        public abstract Task<V> NewFloatFieldView(string fieldName, ViewModel.Field field);
        public abstract Task<V> NewIntegerFieldView(string fieldName, ViewModel.Field field);

        public abstract Task HandleRecursiveViewModel(V parentView, string fieldName, ViewModel.Field field, ViewModel recursiveViewModel);
        public abstract Task HandleSimpleArray(V parentView, string fieldName, ViewModel.Field field, JTokenType arrayType);
        public abstract Task HandleObjectArray(V parentView, string fieldName, ViewModel.Field field, ViewModel entryViewModel);
        public abstract Task HandleMixedObjectArray(V parentView, string fieldName, ViewModel.Field field);

    }

}