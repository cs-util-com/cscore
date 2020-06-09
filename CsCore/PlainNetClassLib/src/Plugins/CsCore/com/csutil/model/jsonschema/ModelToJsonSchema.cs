using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.csutil.model.jsonschema {

    public class ModelToJsonSchema {

        public JsonSerializer jsonSerializer;
        /// <summary>
        /// If true and the view model is derived from an model instance,   
        /// the default will be set to the value of the instance field.
        /// </summary>
        public bool useInstanceValAsDefault = false;
        /// <summary> A dict. of all schenams known to the generator </summary>
        public Dictionary<string, JsonSchema> schemas = new Dictionary<string, JsonSchema>();
        public ISet<string> namespaceBacklist = null;

        public ModelToJsonSchema() {
            var jsonSettings = JsonNetSettings.defaultSettings;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
            jsonSerializer = JsonSerializer.Create(jsonSettings);
        }

        public JsonSchema ToJsonSchema(string modelName, object model) {
            var modelType = model.GetType();
            if (GetExistingSchemaFor(modelType, out JsonSchema vm)) { return vm; }
            return NewJsonSchema(modelName, modelType, model);
        }

        public JsonSchema ToJsonSchema(string modelName, Type modelType) {
            if (GetExistingSchemaFor(modelType, out JsonSchema vm)) { return vm; }
            return NewJsonSchema(modelName, modelType);
        }

        public JsonSchema ToJsonSchema(string modelName, string json) {
            if (schemas.TryGetValue(modelName, out JsonSchema vm)) { return vm; }
            return NewJsonSchema(modelName, JsonReader.GetReader().Read<JObject>(json));
        }

        private bool GetExistingSchemaFor(Type modelType, out JsonSchema vm) {
            return schemas.TryGetValue(ToTypeString(modelType), out vm);
        }

        private JsonSchema NewJsonSchema(string modelName, Type modelType, object model = null) {
            var schema = new JsonSchema() { title = modelName, type = JTokenType.Object.ToJsonSchemaType() };
            return SetupJsonSchema(schema, modelType, model);
        }

        private JsonSchema SetupJsonSchema(JsonSchema schema, Type modelType, object model) {
            schema.modelType = ToTypeString(modelType);
            schemas.Add(schema.modelType, schema);
            schema.properties = new Dictionary<string, JsonSchema>();
            if (model != null) {
                AssertV2.IsTrue(modelType == model.GetType(), $"modelType ({modelType}) != model.type ({model.GetType()})");
                AddFieldsViaJson(schema, model, ToJsonModel(model));
            } else {
                AddFieldsViaReflection(schema, modelType);
            }
            var req = schema.properties.Filter(f => f.Value.mandatory == true).Map(f => f.Key);
            if (!req.IsNullOrEmpty()) { schema.required = req.ToList(); }
            return schema;
        }

        private JsonSchema NewJsonSchema(string modelName, JObject jObject) {
            var schema = new JsonSchema() { title = modelName, type = JTokenType.Object.ToJsonSchemaType() };
            schema.properties = new Dictionary<string, JsonSchema>();
            AddFieldsViaJson(schema, null, jObject);
            return schema;
        }

        private void AddFieldsViaReflection(JsonSchema schema, Type modelType) {
            foreach (var member in modelType.GetMembers()) {
                if (member is FieldInfo || member is PropertyInfo) {
                    var fieldName = member.Name;
                    schema.properties.Add(fieldName, NewField(fieldName, modelType));
                }
            }
        }

        private void AddFieldsViaJson(JsonSchema schema, object model, IEnumerable<KeyValuePair<string, JToken>> jsonModel) {
            foreach (var property in jsonModel) {
                schema.properties.Add(property.Key, NewField(property.Key, model?.GetType(), model, property.Value));
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
                if (model.TryGetCustomAttribute(out ContentAttribute c)) { newField.format = "" + c.type; }
                if (model.TryGetCustomAttribute(out MinMaxRangeAttribute ra)) {
                    newField.minimum = ra.minimum;
                    newField.maximum = ra.maximum;
                }
                if (model.TryGetCustomAttribute(out InputLengthAttribute ila)) {
                    if (ila.minLength > 0) { newField.minLength = ila.minLength; }
                    if (ila.maxLength > 0) { newField.maxLength = ila.maxLength; }
                }
                if (model.TryGetCustomAttribute(out EnumAttribute e)) {
                    newField.contentEnum = e.names;
                    newField.additionalItems = e.allowOtherInput;
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
                    SetupInnerJsonSchema(newField, modelType, modelInstance);
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
                            SetupInnerJsonSchema(childVm, listElemType, firstChildInstance);
                            newField.items = new List<JsonSchema>() { childVm };
                        } else {
                            newField.items = new List<JsonSchema>();
                            foreach (var child in childrenInstances) {
                                var childVm = new JsonSchema() { type = arrayElemJType.ToJsonSchemaType() };
                                SetupInnerJsonSchema(childVm, child.GetType(), child);
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

        private void SetupInnerJsonSchema(JsonSchema innerSchema, Type modelType, object model = null) {
            if (GetExistingSchemaFor(modelType, out JsonSchema vm)) {
                // Schema already generated for this type, so dont traverse modelType:
                innerSchema.modelType = ToTypeString(modelType);
                return;
            }
            if (modelType.IsSystemType()) {
                // Schema for System types (e.g. Dictionary) not traversed:
                innerSchema.modelType = ToTypeString(modelType);
                return;
            }
            if (namespaceBacklist != null && namespaceBacklist.Contains(modelType.Namespace)) { return; }
            SetupJsonSchema(innerSchema, modelType, model);
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