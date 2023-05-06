using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using com.csutil.json;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Zio;
using Newtonsoft.Json.Linq;

namespace com.csutil.model.ecs {

    public class EntityComponentSystem<T> where T : IEntityData {

        private readonly DirectoryEntry EntityDir;
        private readonly JsonDiffPatch JonDiffPatch = new JsonDiffPatch();
        private readonly Dictionary<string, JToken> JTokens = new Dictionary<string, JToken>();

        private Func<JsonSerializer> GetJsonSerializer = () => JsonSerializer.Create(JsonNetSettings.typedJsonSettings);

        public EntityComponentSystem(DirectoryEntry entityDir) {
            this.EntityDir = entityDir;
        }

        public async Task LoadAllJTokens() {
            var jsonSerializer = GetJsonSerializer();
            var tasks = new List<Task>();
            foreach (var templateFile in EntityDir.EnumerateFiles()) {
                tasks.Add(TaskV2.Run((() => LoadJTokenFromFile(templateFile, jsonSerializer))));
            }
            await Task.WhenAll(tasks);
        }

        private void LoadJTokenFromFile(FileEntry templateFile, JsonSerializer jsonSerializer) {
            using (var stream = templateFile.OpenForRead()) {
                JToken templateJToken = jsonSerializer.Deserialize<JToken>(new JsonTextReader(new StreamReader(stream)));
                UpdateJTokens(templateFile.Name, templateJToken);
            }
        }

        private void UpdateJTokens(string id, JToken jtoken) {
            JTokens[id] = jtoken;
        }

        public void Save(T instance) {
            var entityId = instance.GetId();
            entityId.ThrowErrorIfNullOrEmpty("entity.Id");
            var file = EntityDir.GetChild(entityId);
            var json = ToJToken(instance, GetJsonSerializer());
            var templateId = instance.TemplateId;
            if (templateId != null) {
                var template = ComposeFullJToken(templateId);
                json = JonDiffPatch.Diff(template, json);
            }
            UpdateJTokens(entityId, json);
            file.SaveAsJson(json);
        }

        private JToken ToJToken(T instance, JsonSerializer serializer) { return JToken.FromObject(instance, serializer); }

        public T CreateVariantOf(T entity) {
            if (!JTokens.ContainsKey(entity.GetId())) { throw new KeyNotFoundException("Template not found: " + entity.GetId()); }
            JsonSerializer serializer = GetJsonSerializer();
            var json = ToJToken(entity, serializer);
            json["Id"] = "" + GuidV2.NewGuid();
            json["TemplateId"] = entity.GetId();
            return ToObject(json, serializer);
        }

        private T ToObject(JToken json, JsonSerializer serializer) {
            T entity = json.ToObject<T>(serializer);
            AssertAllFieldsWereDeserialized(entity, json);
            return entity;
        }

        [Conditional("DEBUG")]
        private void AssertAllFieldsWereDeserialized(T entity, JToken json) {
            var backtoJToken = ToJToken(entity, GetJsonSerializer());
            var diff = JonDiffPatch.Diff(json, backtoJToken);
            if (diff != null) { throw new Exception("Not all fields were deserialized: " + diff); }
        }

        public IEnumerable<string> GetAllEntityIds() {
            return JTokens.Keys;
        }

        public async Task<T> Load(string entityId) {
            return ToObject(ComposeFullJToken(entityId), GetJsonSerializer());
        }

        /// <summary> Recursively composes the full json for the given entity id by applying the templates </summary>
        private JToken ComposeFullJToken(string entityId) {
            if (!JTokens.ContainsKey(entityId)) { throw new KeyNotFoundException("Entity not found: " + entityId); }
            var json = JTokens[entityId];
            if (json["TemplateId"] is JArray templateIdArray) {
                var templateId = templateIdArray[1].Value<string>();
                var template = ComposeFullJToken(templateId);
                json = JonDiffPatch.Patch(template, json);
            }
            return json;
        }

    }

    public interface IEntityData : HasId {

        string Id { get; }
        string TemplateId { get; }
        Matrix4x4? LocalPose { get; }
        IList<IComponentData> Components { get; }
        IList<string> ChildrenIds { get; }
        IList<string> Tags { get; }

    }

    public interface IComponentData : HasId {

    }

}