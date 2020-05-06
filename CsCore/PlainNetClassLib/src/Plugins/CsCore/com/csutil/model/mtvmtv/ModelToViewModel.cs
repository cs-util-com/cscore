using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace com.csutil.model.mtvmtv {

    [Serializable]
    public class ViewModel {

        public string modelName;
        public string modelType;
        public List<string> order;
        public Dictionary<string, Field> fields;

        [Serializable]
        public class Field {

            public Text text;
            public string type;
            public bool? readOnly;
            public bool? writeOnly;
            public bool? mandatory;
            public string regex;
            /// <summary> If the field is an object it has a view model itself </summary>
            public ViewModel objVm;
            public ChildList children;

            [Serializable]
            public class Text {
                public string name;
                public string descr;
            }

            [Serializable]
            public class ChildList {
                public string type;
                public List<ViewModel> entries;
            }

        }

    }

    public class ModelToViewModel {

        public JsonSerializer jsonSerializer;
        public bool forceSameTypeForAllChildren = false;
        public Dictionary<string, ViewModel> viewModels = new Dictionary<string, ViewModel>();
        public ISet<string> namespaceBacklist = null;

        public ModelToViewModel() {
            var jsonSettings = JsonNetSettings.defaultSettings;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
            jsonSerializer = JsonSerializer.Create(jsonSettings);
        }

        public ViewModel ToViewModel(string modelName, object model) {
            var modelType = model.GetType();
            if (GetExistingViewModelFor(modelType, out ViewModel vm)) { return vm; }
            return NewViewModel(modelName, modelType, model);
        }

        public ViewModel ToViewModel(string modelName, Type modelType) {
            if (GetExistingViewModelFor(modelType, out ViewModel vm)) { return vm; }
            return NewViewModel(modelName, modelType);
        }

        private bool GetExistingViewModelFor(Type modelType, out ViewModel vm) {
            return viewModels.TryGetValue(ToTypeString(modelType), out vm);
        }

        private ViewModel NewViewModel(string modelName, Type modelType, object model = null) {
            var viewModel = new ViewModel() { modelName = modelName, modelType = ToTypeString(modelType) };
            viewModels.Add(viewModel.modelType, viewModel);
            viewModel.fields = new Dictionary<string, ViewModel.Field>();
            if (model != null) {
                AssertV2.IsTrue(modelType == model.GetType(), $"modelType ({modelType}) != model.type ({model.GetType()})");
                AddFieldsViaJson(viewModel, model);
            } else {
                AddFieldsViaReflection(viewModel, modelType);
            }
            return viewModel;
        }

        private void AddFieldsViaReflection(ViewModel viewModel, Type modelType) {
            viewModel.order = new List<string>();
            foreach (var member in modelType.GetMembers()) {
                if (member is FieldInfo || member is PropertyInfo) {
                    var fieldName = member.Name;
                    viewModel.order.Add(fieldName);
                    viewModel.fields.Add(fieldName, NewField(fieldName, modelType));
                }
            }
        }

        private void AddFieldsViaJson(ViewModel viewModel, object model) {
            IEnumerable<KeyValuePair<string, JToken>> jsonModel = ToJsonModel(model);
            viewModel.order = jsonModel.Map(property => property.Key).ToList();
            foreach (var property in jsonModel) {
                viewModel.fields.Add(property.Key, NewField(property.Key, model.GetType(), model, property.Value));
            }
        }

        public JObject ToJsonModel(object model) {
            return JObject.FromObject(model, jsonSerializer);
        }

        public virtual ViewModel.Field NewField(string name, Type parentType, object pInstance = null, JToken jpInstance = null) {
            MemberInfo model = parentType.GetMember(name).First();
            Type modelType = GetFieldOrPropType(model);
            JTokenType jTokenType = ToJTokenType(modelType, jpInstance);
            ViewModel.Field newField = new ViewModel.Field() { type = "" + jTokenType, text = ToTextName(name) };
            if (!model.CanWriteTo()) { newField.readOnly = true; }
            if (jTokenType == JTokenType.Object) {
                var modelInstance = pInstance != null ? model.GetValue(pInstance) : null;
                newField.objVm = NewInnerViewModel(name, modelType, modelInstance);
            }
            if (jTokenType == JTokenType.Array) {
                SetupFieldAsArray(newField, GetListElementType(modelType), GetChildrenArray(pInstance, model));
            }
            return newField;
        }

        private ViewModel.Field.Text ToTextName(string name) {
            name = RegexTemplates.SplitCamelCaseString(name);
            return new ViewModel.Field.Text() { name = name };
        }

        private Type GetListElementType(Type listType) {
            if (listType.IsArray) { return listType.GetElementType(); }
            var args = listType.GetGenericArguments();
            AssertV2.IsTrue(args.Count() == 1, "Not 1 generic list type, instead: " + args.ToStringV2(x => "" + x));
            return args.Single();
        }

        private void SetupFieldAsArray(ViewModel.Field field, Type arrayElemType, object[] childrenInstances) {
            field.children = new ViewModel.Field.ChildList();
            var arrayElemJType = ToJTokenType(arrayElemType);
            field.children.type = "" + arrayElemJType;
            if (!IsSimpleType(arrayElemJType)) {
                if (childrenInstances == null || AllChildrenHaveSameType(childrenInstances)) {
                    var firstChildInstance = childrenInstances?.FirstOrDefault();
                    var childVm = NewInnerViewModel(modelName: "EntryType", arrayElemType, firstChildInstance);
                    field.children.entries = new List<ViewModel>() { childVm };
                } else {
                    AddAllChildrenViewModels(field, childrenInstances);
                }
            }
        }

        private object[] GetChildrenArray(object parentInstance, MemberInfo model) {
            if (parentInstance == null) { return null; }
            IEnumerable children = model.GetValue(parentInstance) as IEnumerable;
            if (children == null) { return null; }
            return children.Cast<object>().ToArray();
        }

        public bool IsSimpleType(JTokenType type) {
            return type is JTokenType.Boolean || type is JTokenType.Integer ||
                    type is JTokenType.Float || type is JTokenType.String;
        }

        private JTokenType ToJTokenType(Type elemType, JToken jpInstance = null) {
            if (jpInstance != null && jpInstance.Type != JTokenType.Null) { return jpInstance.Type; }
            return ToJTokenType(elemType);
        }

        private JTokenType ToJTokenType(Type elemType) {
            if (elemType.IsCastableTo<bool>()) { return JTokenType.Boolean; }
            if (elemType.IsWholeNumberType()) { return JTokenType.Integer; }
            if (elemType.IsDecimalNumberType()) { return JTokenType.Float; }
            if (elemType.IsCastableTo<string>()) { return JTokenType.String; }
            if (elemType.IsCastableTo<IDictionary>()) { return JTokenType.Object; }
            if (elemType.IsCastableTo<IEnumerable>()) { return JTokenType.Array; }
            return JTokenType.Object;
        }

        private bool AllChildrenHaveSameType(object[] childrenInstances) {
            if (forceSameTypeForAllChildren) { return true; }
            if (childrenInstances.IsNullOrEmpty()) { return true; }
            Type childrenType = childrenInstances.First().GetType();
            return childrenInstances.All(c => c.GetType() == childrenType);
        }

        private void AddAllChildrenViewModels(ViewModel.Field arrayField, object[] childrenInstances) {
            arrayField.children.entries = new List<ViewModel>();
            for (int i = 0; i < childrenInstances.Length; i++) {
                var child = childrenInstances[i];
                arrayField.children.entries.Add(NewInnerViewModel("" + i, child.GetType(), child));
            }
            AssertV2.AreEqual(childrenInstances.Length, arrayField.children.entries.Count);
        }

        private ViewModel NewInnerViewModel(string modelName, Type modelType, object model = null) {
            if (GetExistingViewModelFor(modelType, out ViewModel vm)) {
                // ViewModel already generated for this type, so dont traverse modelType:
                return new ViewModel() { modelType = ToTypeString(modelType) };
            }
            if (modelType.IsSystemType()) {
                // ViewModel for System types (e.g. Dictionary) not traversed:
                return new ViewModel() { modelType = ToTypeString(modelType) };
            }
            if (namespaceBacklist != null && namespaceBacklist.Contains(modelType.Namespace)) { return null; }
            return NewViewModel(modelName, modelType, model);
        }

        private string ToTypeString(Type type) { return type.ToString(); }

        private static Type GetFieldOrPropType(MemberInfo info) {
            if (info is FieldInfo f) { return f.FieldType; }
            if (info is PropertyInfo p) { return p.PropertyType; }
            return null;
        }

    }

}