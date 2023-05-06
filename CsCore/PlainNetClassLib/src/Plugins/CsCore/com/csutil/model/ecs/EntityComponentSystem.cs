using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using com.csutil.json;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Zio;
using Newtonsoft.Json.Linq;

namespace com.csutil.model.ecs {

    public static class IEntityExtensions {

        public static IEnumerable<IEntity<T>> GetChildren<T>(this IEntity<T> self) where T : IEntityData {
            return self.ChildrenIds.Map(x => self.Ecs.GetEntity(x));
        }

        public static IEntity<T> GetParent<T>(this IEntity<T> self) where T : IEntityData {
            return self.Ecs.GetParentOf(self.Id);
        }

        public static IEntity<T> AddChild<T>(this IEntity<T> parent, T childData, Func<IEntity<T>, T, T> mutateChildrenListInParentEntity) where T : IEntityData {
            var newChild = parent.Ecs.Add(childData);
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent, childData));
            return newChild;
        }

    }

    public class EntityComponentSystem<T> where T : IEntityData {

        private class Entity : IEntity<T> {

            public string GetId() { return Data.GetId(); }
            public T Data { get; set; }
            public EntityComponentSystem<T> Ecs { get; set; }

            public string Id => Data.Id;
            public string TemplateId => Data.TemplateId;
            public Matrix4x4? LocalPose => Data.LocalPose;
            public IReadOnlyList<IComponentData> Components => Data.Components;
            public IReadOnlyList<string> ChildrenIds => Data.ChildrenIds;
            public IReadOnlyList<string> Tags => Data.Tags;

        }

        private readonly TemplatesIO<T> TemplatesIo;
        private readonly Dictionary<string, Entity> Entities = new Dictionary<string, Entity>();
        private readonly Dictionary<string, string> ParentIds = new Dictionary<string, string>();

        public EntityComponentSystem(TemplatesIO<T> templatesIo) {
            TemplatesIo = templatesIo;
        }

        public IEntity<T> Add(T entityData) {
            return AddEntity(new Entity() { Data = entityData, Ecs = this });
        }

        private IEntity<T> AddEntity(Entity entity) {
            var entityId = entity.Id;
            entityId.ThrowErrorIfNullOrEmpty("entityData.Id");
            Entities[entityId] = entity;
            UpdateParentIds(entityId, entity);
            return entity;
        }

        private void UpdateParentIds(string parentId, Entity parent) {
            if (parent.ChildrenIds != null) {
                foreach (var childId in parent.ChildrenIds) { ParentIds[childId] = parentId; }
            }
        }

        public void Update(T updatedEntityData) {
            var entityId = updatedEntityData.Id;
            var entity = Entities[entityId];
            var oldEntry = entity.Data;
            entity.Data = updatedEntityData;

            // Remove outdated parent ids and add new ones:
            if (oldEntry.ChildrenIds != null) {
                var removedChildrenIds = oldEntry.ChildrenIds.Except(updatedEntityData.ChildrenIds);
                foreach (var childId in removedChildrenIds) { ParentIds.Remove(childId); }
            }
            UpdateParentIds(entityId, entity);
        }

        public async Task LoadSceneGraphFromDisk() {
            await TemplatesIo.LoadAllTemplateFilesIntoMemory();
        }

        public IEntity<T> GetEntity(string entityId) {
            return Entities[entityId];
        }

        public IEntity<T> GetParentOf(string childId) {
            return GetEntity(ParentIds[childId]);
        }

    }

    public class TemplatesIO<T> where T : IEntityData {

        private readonly DirectoryEntry EntityDir;
        private readonly JsonDiffPatch JonDiffPatch = new JsonDiffPatch();

        /// <summary> A cache of all loaded templates as they are stored on disk, these need to be combined with all parents to get the full entity data </summary>
        private readonly Dictionary<string, JToken> LoadedTemplates = new Dictionary<string, JToken>();

        private Func<JsonSerializer> GetJsonSerializer = () => JsonSerializer.Create(JsonNetSettings.typedJsonSettings);

        public TemplatesIO(DirectoryEntry entityDir) {
            this.EntityDir = entityDir;
        }

        /// <summary> Loads all template files from disk into memory </summary>
        public async Task LoadAllTemplateFilesIntoMemory() {
            var jsonSerializer = GetJsonSerializer();
            var tasks = new List<Task>();
            foreach (var templateFile in EntityDir.EnumerateFiles()) {
                tasks.Add(TaskV2.Run((() => LoadJTokenFromFile(templateFile, jsonSerializer))));
            }
            await Task.WhenAll(tasks);
        }

        private void LoadJTokenFromFile(FileEntry templateFile, JsonSerializer jsonSerializer) {
            var templateId = templateFile.Name;
            if (LoadedTemplates.ContainsKey(templateId)) { return; }
            using (var stream = templateFile.OpenForRead()) {
                JToken template = jsonSerializer.Deserialize<JToken>(new JsonTextReader(new StreamReader(stream)));
                UpdateTemplateCache(templateId, template);
            }
        }

        private void UpdateTemplateCache(string id, JToken template) {
            LoadedTemplates[id] = template;
        }

        public void SaveAsTemplate(T instance) {
            var entityId = instance.GetId();
            entityId.ThrowErrorIfNullOrEmpty("entity.Id");
            var file = GetEntityFileForId(entityId);
            var json = ToJToken(instance, GetJsonSerializer());
            var templateId = instance.TemplateId;
            if (templateId != null) {
                var template = ComposeFullJson(templateId, allowLazyLoadFromDisk: true);
                json = JonDiffPatch.Diff(template, json);
            }
            file.SaveAsJson(json);
            UpdateTemplateCache(entityId, json);
        }

        private FileEntry GetEntityFileForId(string entityId) {
            return EntityDir.GetChild(entityId);
        }

        public void Delete(string entityId) {
            if (LoadedTemplates.Remove(entityId)) {
                GetEntityFileForId(entityId).DeleteV2();
            }
        }

        private JToken ToJToken(T instance, JsonSerializer serializer) {
            return JToken.FromObject(instance, serializer);
        }

        public T CreateVariantInstanceOf(T template) {
            var templateId = template.GetId();
            if (!LoadedTemplates.ContainsKey(templateId)) {
                throw new InvalidOperationException("The passed instance first needs to be stored as a template");
            }
            JsonSerializer serializer = GetJsonSerializer();
            var json = ToJToken(template, serializer);
            json["Id"] = "" + GuidV2.NewGuid();
            json["TemplateId"] = templateId;
            return ToObject(json, serializer);
        }

        private T ToObject(JToken json, JsonSerializer serializer) {
            T entity = json.ToObject<T>(serializer);
            AssertAllFieldsWereDeserialized(json, entity);
            return entity;
        }

        [Conditional("DEBUG")]
        private void AssertAllFieldsWereDeserialized(JToken sourceJson, T resultingEntity) {
            var backAsJson = ToJToken(resultingEntity, GetJsonSerializer());
            var diff = JonDiffPatch.Diff(sourceJson, backAsJson);
            if (diff != null) { throw new Exception("Not all props were deserialized, missing set/get for:" + diff); }
        }

        public IEnumerable<string> GetAllEntityIds() {
            return EntityDir.EnumerateFiles().Map(x => x.Name);
        }

        /// <summary> Creates a template instance based on the involved templates </summary>
        /// <param name="entityId"> The id of the entity to load </param>
        /// <param name="allowLazyLoadFromDisk"> if false its is expected all entities were already loaded into memory via <see cref="LoadAllTemplateFilesIntoMemory"/> </param>
        public T LoadTemplateInstance(string entityId, bool allowLazyLoadFromDisk = true) {
            return ToObject(ComposeFullJson(entityId, allowLazyLoadFromDisk), GetJsonSerializer());
        }

        /// <summary> Recursively composes the full json for the given entity id by applying the templates </summary>
        private JToken ComposeFullJson(string entityId, bool allowLazyLoadFromDisk) {
            if (!LoadedTemplates.ContainsKey(entityId)) {
                if (allowLazyLoadFromDisk) {
                    LoadJTokenFromFile(GetEntityFileForId(entityId), GetJsonSerializer());
                } else {
                    throw new KeyNotFoundException("Entity not found: " + entityId);
                }
            }
            var json = LoadedTemplates[entityId];
            if (json["TemplateId"] is JArray templateIdArray) {
                var templateId = templateIdArray[1].Value<string>();
                var template = ComposeFullJson(templateId, allowLazyLoadFromDisk);
                json = JonDiffPatch.Patch(template, json);
            }
            return json;
        }

    }

    public interface IEntity<T> : IEntityData where T : IEntityData {

        T Data { get; }
        EntityComponentSystem<T> Ecs { get; }

    }

    public interface IEntityData : HasId {

        string Id { get; }
        string TemplateId { get; }
        Matrix4x4? LocalPose { get; }
        IReadOnlyList<IComponentData> Components { get; }
        IReadOnlyList<string> ChildrenIds { get; }
        IReadOnlyList<string> Tags { get; }

    }

    public interface IComponentData : HasId {

    }

}