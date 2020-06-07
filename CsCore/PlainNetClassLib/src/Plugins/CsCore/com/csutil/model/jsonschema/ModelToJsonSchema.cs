using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.csutil.model.mtvmtv {

    public class ModelToJsonSchema {

        public JsonSerializer jsonSerializer;
        /// <summary>
        /// If true and the view model is derived from an model instance,   
        /// the default will be set to the value of the instance field.
        /// </summary>
        public bool useInstanceValAsDefault = false;
        public Dictionary<string, JsonSchema> viewModels = new Dictionary<string, JsonSchema>();
        public ISet<string> namespaceBacklist = null;

        public ModelToJsonSchema() {
            var jsonSettings = JsonNetSettings.defaultSettings;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
            jsonSerializer = JsonSerializer.Create(jsonSettings);
        }

        public JsonSchema ToViewModel(string modelName, object model) {
            var modelType = model.GetType();
            if (GetExistingViewModelFor(modelType, out JsonSchema vm)) { return vm; }
            return NewViewModel(modelName, modelType, model);
        }

        public JsonSchema ToViewModel(string modelName, Type modelType) {
            if (GetExistingViewModelFor(modelType, out JsonSchema vm)) { return vm; }
            return NewViewModel(modelName, modelType);
        }

        public JsonSchema ToViewModel(string modelName, string json) {
            if (viewModels.TryGetValue(modelName, out JsonSchema vm)) { return vm; }
            return NewViewModel(modelName, JsonReader.GetReader().Read<JObject>(json));
        }

        private bool GetExistingViewModelFor(Type modelType, out JsonSchema vm) {
            return viewModels.TryGetValue(ToTypeString(modelType), out vm);
        }

        private JsonSchema NewViewModel(string modelName, Type modelType, object model = null) {
            var viewModel = new JsonSchema() { title = modelName, type = JTokenType.Object.ToJsonSchemaType() };
            return SetupViewModel(viewModel, modelType, model);
        }

        private JsonSchema SetupViewModel(JsonSchema viewModel, Type modelType, object model) {
            viewModel.modelType = ToTypeString(modelType);
            viewModels.Add(viewModel.modelType, viewModel);
            viewModel.properties = new Dictionary<string, JsonSchema>();
            if (model != null) {
                AssertV2.IsTrue(modelType == model.GetType(), $"modelType ({modelType}) != model.type ({model.GetType()})");
                AddFieldsViaJson(viewModel, model, ToJsonModel(model));
            } else {
                AddFieldsViaReflection(viewModel, modelType);
            }
            var req = viewModel.properties.Filter(f => f.Value.mandatory == true).Map(f => f.Key);
            if (!req.IsNullOrEmpty()) { viewModel.required = req.ToList(); }
            return viewModel;
        }

        private JsonSchema NewViewModel(string modelName, JObject jObject) {
            var viewModel = new JsonSchema() { title = modelName, type = JTokenType.Object.ToJsonSchemaType() };
            viewModel.properties = new Dictionary<string, JsonSchema>();
            AddFieldsViaJson(viewModel, null, jObject);
            return viewModel;
        }

        private void AddFieldsViaReflection(JsonSchema viewModel, Type modelType) {
            viewModel.order = new List<string>();
            foreach (var member in modelType.GetMembers()) {
                if (member is FieldInfo || member is PropertyInfo) {
                    var fieldName = member.Name;
                    viewModel.order.Add(fieldName);
                    viewModel.properties.Add(fieldName, NewField(fieldName, modelType));
                }
            }
        }

        private void AddFieldsViaJson(JsonSchema viewModel, object model, IEnumerable<KeyValuePair<string, JToken>> jsonModel) {
            viewModel.order = jsonModel.Map(property => property.Key).ToList();
            foreach (var property in jsonModel) {
                viewModel.properties.Add(property.Key, NewField(property.Key, model?.GetType(), model, property.Value));
            }
        }

        public JObject ToJsonModel(object model) { return JObject.FromObject(model, jsonSerializer); }

        public virtual JsonSchema NewField(string name, Type parentType, object pInstance = null, JToken jpInstance = null) {
            MemberInfo model = parentType?.GetMember(name).First();
            Type modelType = GetModelType(model);
            JTokenType jTokenType = ToJTokenType(modelType, jpInstance);
            AssertV2.IsNotNull(jTokenType, "jTokenType");
            JsonSchema newField = new JsonSchema() { type = jTokenType.ToJsonSchemaType(), title = JsonSchema.ToTitle(name) };
            ExtractFieldDocu(newField, model, modelType, jTokenType, pInstance, jpInstance);
            if (model != null) {
                if (!model.CanWriteTo()) { newField.readOnly = true; }
                if (model.TryGetCustomAttribute(out RegexAttribute attr)) { newField.pattern = attr.regex; }
                if (model.TryGetCustomAttribute(out ContentAttribute c)) { newField.contentType = "" + c.type; }
                if (model.TryGetCustomAttribute(out MinMaxRangeAttribute ra)) {
                    newField.minimum = ra.minimum;
                    newField.maximum = ra.maximum;
                }
                if (model.TryGetCustomAttribute(out EnumAttribute e)) {
                    newField.contentEnum = e.names;
                    newField.additionalItems = e.additionalItems;
                }
                if (model.TryGetCustomAttribute(out RequiredAttribute r)) { newField.mandatory = true; }
                if (model.TryGetCustomAttribute(out JsonPropertyAttribute p)) {
                    if (p.Required == Required.Always || p.Required == Required.DisallowNull) {
                        newField.mandatory = true;
                    }
                }

                if (modelType.IsEnum) { newField.contentEnum = Enum.GetNames(modelType); }
            }
            if (jTokenType == JTokenType.Object) {
                if (modelType == null) {

                    newField.properties = new Dictionary<string, JsonSchema>();
                    AddFieldsViaJson(newField, null, jpInstance as JObject);

                } else {
                    var modelInstance = pInstance != null ? model.GetValue(pInstance) : null;
                    SetupInnerViewModel(newField, modelType, modelInstance);
                }
            }
            if (jTokenType == JTokenType.Array) {
                var listElemType = GetListElementType(modelType);
                var arrayElemJType = ToJTokenType(listElemType);
                if (arrayElemJType == JTokenType.Null) {
                    if (jpInstance is JArray a && a.Count > 0) { arrayElemJType = a.First.Type; }
                }
                if (arrayElemJType != JTokenType.Null) {
                    if (!IsSimpleType(arrayElemJType)) {
                        var childrenInstances = GetChildrenArray(pInstance, jpInstance, model);
                        if (childrenInstances == null || AllChildrenHaveSameType(childrenInstances)) {
                            var firstChildInstance = childrenInstances?.FirstOrDefault();
                            var childVm = new JsonSchema() { type = arrayElemJType.ToJsonSchemaType() };
                            SetupInnerViewModel(childVm, listElemType, firstChildInstance);
                            newField.items = new List<JsonSchema>() { childVm };
                        } else {
                            newField.items = new List<JsonSchema>();
                            foreach (var child in childrenInstances) {
                                var childVm = new JsonSchema() { type = arrayElemJType.ToJsonSchemaType() };
                                SetupInnerViewModel(childVm, child.GetType(), child);
                                newField.items.Add(childVm);
                            }
                            AssertV2.AreEqual(childrenInstances.Length, newField.items.Count);
                        }
                    } else {
                        newField.items = new List<JsonSchema>() { new JsonSchema() { type = arrayElemJType.ToJsonSchemaType() } };
                    }
                }
            }
            return newField;
        }

        public virtual bool ExtractFieldDocu(JsonSchema field, MemberInfo m, Type modelType, JTokenType t, object pInstance, JToken jpInstance) {
            var descrAttr = m?.GetCustomAttribute<DescriptionAttribute>(true);
            if (descrAttr != null) {
                field.description = descrAttr.description;
                field.defaultVal = descrAttr.defaultVal;
                return true;
            }
            if (IsSimpleType(t)) {
                if (pInstance != null) {
                    var value = m.GetValue(pInstance);
                    field.description = $"e.g. '{value}'";
                    if (useInstanceValAsDefault) { field.defaultVal = "" + value; }
                    return true;
                }
                if (jpInstance != null) {
                    field.description = $"e.g. '{jpInstance}'";
                    if (useInstanceValAsDefault) { field.defaultVal = "" + jpInstance; }
                    return true;
                }
                if (t == JTokenType.Integer) {
                    if (modelType != null && modelType.IsEnum) { return false; }
                    field.description = "e.g. 99";
                    return true;
                }
                if (t == JTokenType.Float) {
                    field.description = "e.g. " + 1.23f;
                    return true;
                }
            }
            return false;
        }

        private Type GetListElementType(Type listType) {
            if (listType == null) { return null; }
            if (listType.IsArray) { return listType.GetElementType(); }
            var args = listType.GetGenericArguments();
            AssertV2.IsTrue(args.Length == 1, "Not 1 generic list type, instead: " + args.ToStringV2(x => "" + x));
            return args.Single();
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
            if (childrenInstances.IsNullOrEmpty()) { return true; }
            Type childrenType = childrenInstances.First().GetType();
            return childrenInstances.All(c => c.GetType() == childrenType);
        }

        private void SetupInnerViewModel(JsonSchema viewModel, Type modelType, object model = null) {
            if (GetExistingViewModelFor(modelType, out JsonSchema vm)) {
                // ViewModel already generated for this type, so dont traverse modelType:
                viewModel.modelType = ToTypeString(modelType);
                return;

            }
            if (modelType.IsSystemType()) {
                // ViewModel for System types (e.g. Dictionary) not traversed:
                viewModel.modelType = ToTypeString(modelType);
                return;
            }
            if (namespaceBacklist != null && namespaceBacklist.Contains(modelType.Namespace)) { return; }
            SetupViewModel(viewModel, modelType, model);
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