using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace com.csutil.model.ecs {

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
        private readonly Dictionary<string, IEntity<T>> Entities = new Dictionary<string, IEntity<T>>();
        private readonly Dictionary<string, string> ParentIds = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, IEntity<T>> AllEntities => Entities;
        public IReadOnlyDictionary<string, string> AllParentIds => ParentIds;

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

        private void UpdateParentIds(string parentId, IEntity<T> parent) {
            if (parent.ChildrenIds != null) {
                foreach (var childId in parent.ChildrenIds) { ParentIds[childId] = parentId; }
            }
        }

        public void Update(T updatedEntityData) {
            var entityId = updatedEntityData.Id;
            var entity = (Entity)Entities[entityId];

            var oldEntry = entity.Data;
            entity.Data = updatedEntityData;

            if (ParentIds.TryGetValue(entityId, out string parentId)) {
                // Remove from ParentIds cache if parent does not contain the child anymore:
                if (!Entities[parentId].Data.ChildrenIds.Contains(entityId)) { ParentIds.Remove(entityId); }
            }
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

        public void Destroy(string entityId) {
            var entity = Entities[entityId] as Entity;
            Entities.Remove(entityId);
            ParentIds.Remove(entityId);
            entity.Ecs = null;
            if (entity.Data.Components != null) {
                foreach (var comp in entity.Data.Components) {
                    DisposeIfPossible(comp);
                }
            }
            DisposeIfPossible(entity.Data);
        }

        private static void DisposeIfPossible(object x) {
            if (x is IDisposableV2 d2) {
                d2.DisposeV2();
            } else if (x is IDisposable d1) {
                d1.Dispose();
            }
        }

    }

}