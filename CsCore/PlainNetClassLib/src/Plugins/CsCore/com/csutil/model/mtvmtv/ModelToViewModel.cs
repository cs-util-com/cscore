using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.csutil.model.mtvmtv {

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

        public ViewModel ToViewModel(string modelName, string json) {
            if (viewModels.TryGetValue(modelName, out ViewModel vm)) { return vm; }
            return NewViewModel(modelName, JsonReader.GetReader().Read<JObject>(json));
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

        private ViewModel NewViewModel(string modelName, JObject jObject) {
            var viewModel = new ViewModel() { modelName = modelName };
            viewModel.fields = new Dictionary<string, ViewModel.Field>();
            AddFieldsViaJson(viewModel, null, jObject);
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
            AddFieldsViaJson(viewModel, model, ToJsonModel(model));
        }

        private void AddFieldsViaJson(ViewModel viewModel, object model, IEnumerable<KeyValuePair<string, JToken>> jsonModel) {
            viewModel.order = jsonModel.Map(property => property.Key).ToList();
            foreach (var property in jsonModel) {
                viewModel.fields.Add(property.Key, NewField(property.Key, model?.GetType(), model, property.Value));
            }
        }

        public JObject ToJsonModel(object model) { return JObject.FromObject(model, jsonSerializer); }

        public virtual ViewModel.Field NewField(string name, Type parentType, object pInstance = null, JToken jpInstance = null) {
            MemberInfo model = parentType?.GetMember(name).First();
            Type modelType = GetModelType(model);
            JTokenType jTokenType = ToJTokenType(modelType, jpInstance);
            AssertV2.NotNull(jTokenType, "jTokenType");
            ViewModel.Field newField = new ViewModel.Field() { type = "" + jTokenType, text = ToTextName(name) };
            if (TryGetDescription(model, modelType, jTokenType, pInstance, jpInstance, out string description)) {
                newField.text.descr = description;
            }
            if (model != null) {
                if (model.TryGetCustomAttributes(out IEnumerable<RegexAttribute> attr)) {
                    newField.regex = attr.Filter(x => x?.regex != null).SelectMany(x => x.regex).ToArray();
                }
                if (!model.CanWriteTo()) { newField.readOnly = true; }
                if (model.TryGetCustomAttribute(out ContentAttribute content)) { newField.contentType = "" + content.type; }
                if (model.TryGetCustomAttribute(out EnumAttribute e)) { newField.contentEnum = e.names; }
                if (modelType.IsEnum) { newField.contentEnum = Enum.GetNames(modelType); }
            }
            if (jTokenType == JTokenType.Object) {
                if (modelType == null) {
                    newField.objVm = NewViewModel(name, jpInstance as JObject);
                } else {
                    var modelInstance = pInstance != null ? model.GetValue(pInstance) : null;
                    newField.objVm = NewInnerViewModel(name, modelType, modelInstance);
                }
            }
            if (jTokenType == JTokenType.Array) {
                var listElemType = GetListElementType(modelType);
                newField.children = new ViewModel.Field.ChildList();
                var arrayElemJType = ToJTokenType(listElemType);
                if (arrayElemJType == JTokenType.Null) {
                    if (jpInstance is JArray a && a.Count > 0) { arrayElemJType = a.First.Type; }
                }
                if (arrayElemJType != JTokenType.Null) {
                    newField.children.type = "" + arrayElemJType;
                    if (!IsSimpleType(arrayElemJType)) {
                        SetupFieldAsArray(newField, listElemType, GetChildrenArray(pInstance, jpInstance, model));
                    }
                }
            }
            return newField;
        }

        public virtual bool TryGetDescription(MemberInfo m, Type modelType, JTokenType t, object pInstance, JToken jpInstance, out string result) {
            if (IsSimpleType(t)) {
                var description = m?.GetCustomAttribute<DescriptionAttribute>(true)?.description;
                if (description != null) {
                    result = description;
                    return true;
                }
                if (pInstance != null) {
                    result = $"e.g. '{m.GetValue(pInstance)}'";
                    return true;
                }
                if (jpInstance != null) {
                    result = $"e.g. '{jpInstance}'";
                    return true;
                }
                if (t == JTokenType.Integer) {
                    if (modelType != null && modelType.IsEnum) {
                        result = null;
                        return false;
                    }
                    result = "e.g. 99";
                    return true;
                }
                if (t == JTokenType.Float) {
                    result = "e.g. " + 1.23f;
                    return true;
                }
            }
            result = null;
            return false;
        }

        private ViewModel.Field.Text ToTextName(string name) {
            name = RegexTemplates.SplitCamelCaseString(name);
            return new ViewModel.Field.Text() { name = name };
        }

        private Type GetListElementType(Type listType) {
            if (listType == null) { return null; }
            if (listType.IsArray) { return listType.GetElementType(); }
            var args = listType.GetGenericArguments();
            AssertV2.IsTrue(args.Length == 1, "Not 1 generic list type, instead: " + args.ToStringV2(x => "" + x));
            return args.Single();
        }

        private void SetupFieldAsArray(ViewModel.Field field, Type arrayElemType, object[] childrenInstances) {
            if (childrenInstances == null || AllChildrenHaveSameType(childrenInstances)) {
                var firstChildInstance = childrenInstances?.FirstOrDefault();
                var childVm = NewInnerViewModel(modelName: "EntryType", arrayElemType, firstChildInstance);
                field.children.entries = new List<ViewModel>() { childVm };
            } else {
                AddAllChildrenViewModels(field, childrenInstances);
            }
        }

        private object[] GetChildrenArray(object parentInstance, JToken jpInstance, MemberInfo model) {
            if (parentInstance == null) { return jpInstance?.ToArray(); }
            IEnumerable children = model.GetValue(parentInstance) as IEnumerable;
            if (children == null) { return null; }
            return children.Cast<object>().ToArray();
        }

        public bool IsSimpleType(JTokenType t) {
            return t is JTokenType.Boolean || t is JTokenType.Integer || t is JTokenType.Float || t is JTokenType.String;
        }

        private JTokenType ToJTokenType(Type elemType, JToken jpInstance = null) {
            if (jpInstance != null && jpInstance.Type != JTokenType.Null) { return jpInstance.Type; }
            return ToJTokenType(elemType);
        }

        private JTokenType ToJTokenType(Type elemType) {
            if (elemType == null) { return JTokenType.Null; }
            if (elemType.IsCastableTo<bool>()) { return JTokenType.Boolean; }
            if (elemType.IsWholeNumberType()) { return JTokenType.Integer; }
            if (elemType.IsDecimalNumberType()) { return JTokenType.Float; }
            if (elemType.IsCastableTo<string>()) { return JTokenType.String; }
            if (elemType.IsCastableTo<IDictionary>()) { return JTokenType.Object; }
            if (elemType.IsCastableTo<IEnumerable>()) { return JTokenType.Array; }
            if (elemType.IsEnum) { return JTokenType.Integer; }
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

        private static Type GetModelType(MemberInfo model) {
            Type modelType = GetFieldOrPropType(model);
            if (modelType != null) {
                var nullableType = Nullable.GetUnderlyingType(modelType);
                if (nullableType != null) { modelType = nullableType; }
            }
            return modelType;
        }

        private static Type GetFieldOrPropType(MemberInfo info) {
            if (info is FieldInfo f) { return f.FieldType; }
            if (info is PropertyInfo p) { return p.PropertyType; }
            return null;
        }

    }

}