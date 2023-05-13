using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace com.csutil.model.ecs {

    public class EntityComponentSystem<T> : IDisposableV2 where T : IEntityData {

        private class Entity : IEntity<T> {

            public string GetId() { return Data.GetId(); }
            public T Data { get; set; }
            public EntityComponentSystem<T> Ecs { get; set; }

            public string Id => Data.Id;
            public string Name => Data.Name;
            public string TemplateId => Data.TemplateId;
            public Matrix4x4? LocalPose => Data.LocalPose;
            public IReadOnlyList<IComponentData> Components => Data.Components;
            public IReadOnlyList<string> ChildrenIds => Data.ChildrenIds;

        }

        public DisposeState IsDisposed { get; set; } = DisposeState.Active;

        private readonly TemplatesIO<T> TemplatesIo;
        private readonly Dictionary<string, IEntity<T>> Entities = new Dictionary<string, IEntity<T>>();
        private readonly Dictionary<string, string> ParentIds = new Dictionary<string, string>();
        private readonly Dictionary<string, HashSet<string>> Variants = new Dictionary<string, HashSet<string>>();

        public IReadOnlyDictionary<string, IEntity<T>> AllEntities => Entities;
        
        /// <summary> Lookup of parent id for a given child id </summary>
        public IReadOnlyDictionary<string, string> AllParentIds => ParentIds;

        /// <summary> Triggered when the entity is directly or indirectly changed (e.g. when a template entity is changed).
        /// Will path the IEntity wrapper, the old and the new data </summary>
        public event IEntityUpdateListener OnIEntityUpdated;

        /// <summary>
        /// 
        /// </summary>
        public delegate void IEntityUpdateListener(IEntity<T> iEntityWrapper, UpdateType type, T oldState, T newState);

        public enum UpdateType { Add, Remove, Update }

        public EntityComponentSystem(TemplatesIO<T> templatesIo) {
            TemplatesIo = templatesIo;
        }

        public void Dispose() {
            TemplatesIo.DisposeV2();
            Entities.Clear();
            ParentIds.Clear();
        }

        public IEntity<T> Add(T entityData) {
            return AddEntity(new Entity() { Data = entityData, Ecs = this });
        }

        private IEntity<T> AddEntity(Entity entity) {
            var entityId = entity.Id;
            entityId.ThrowErrorIfNullOrEmpty("entityData.Id");
            var hasOldEntity = Entities.TryGetValue(entityId, out var oldEntity);
            T oldEntityData = hasOldEntity ? oldEntity.Data : default;
            Entities[entityId] = entity;
            UpdateVariantsLookup(entity.Data);
            UpdateParentIds(entityId, entity);
            OnIEntityUpdated?.Invoke(entity, UpdateType.Add, oldEntityData, entity.Data);
            return entity;
        }

        private void UpdateVariantsLookup(T entity) {
            if (entity.TemplateId != null) {
                Variants.AddToValues(entity.TemplateId, entity.Id);
            }
        }

        private void UpdateParentIds(string parentId, IEntity<T> parent) {
            if (parent.ChildrenIds != null) {
                foreach (var childId in parent.ChildrenIds) { ParentIds[childId] = parentId; }
            }
        }

        public void Update(T updatedEntityData) {
            var entityId = updatedEntityData.Id;
            var entity = (Entity)Entities[entityId];

            var oldEntryData = entity.Data;
            entity.Data = updatedEntityData;

            if (ParentIds.TryGetValue(entityId, out string parentId)) {
                // Remove from ParentIds cache if parent does not contain the child anymore:
                if (!Entities[parentId].Data.ChildrenIds.Contains(entityId)) { ParentIds.Remove(entityId); }
            }

            // e.g. if the entries are mutable this will often be true:
            var oldAndNewSameEntry = ReferenceEquals(oldEntryData, updatedEntityData);

            // Remove outdated parent ids and add new ones:
            if (!oldAndNewSameEntry && oldEntryData.ChildrenIds != null) {
                var removedChildrenIds = oldEntryData.ChildrenIds.Except(updatedEntityData.ChildrenIds);
                foreach (var childId in removedChildrenIds) { ParentIds.Remove(childId); }
            }
            UpdateParentIds(entityId, entity);

            if (!oldAndNewSameEntry) {
                // Compute json diff to know if the entry really changed and if not skip informing all variants about the change:
                // This can happen eg if the variant overwrites the field that was just changed in the template
                if (!TemplatesIo.HasChanges(oldEntryData, updatedEntityData)) { return; }
            }

            // At this point in the update method it is known that the entity really changed and 
            OnIEntityUpdated?.Invoke(entity, UpdateType.Update, oldEntryData, updatedEntityData);

            // If the entry is a template for other entries, then all variants need to be updated:
            if (Variants.TryGetValue(updatedEntityData.Id, out var variantIds)) {
                foreach (var variantId in variantIds) {
                    var newVariantState = TemplatesIo.LoadTemplateInstance(variantId);
                    Update(newVariantState);
                }
            }
        }

        public async Task LoadSceneGraphFromDisk() {
            await TemplatesIo.LoadAllTemplateFilesIntoMemory();
            foreach (var entityId in TemplatesIo.GetAllEntityIds()) {
                Add(TemplatesIo.LoadTemplateInstance(entityId));
            }
        }

        public IEntity<T> GetEntity(string entityId) {
            return Entities[entityId];
        }

        public IEntity<T> GetParentOf(string childId) {
            return GetEntity(ParentIds[childId]);
        }

        public void Destroy(string entityId) {
            var entity = Entities[entityId] as Entity;
            OnIEntityUpdated?.Invoke(entity, UpdateType.Remove, entity.Data, default);
            Entities.Remove(entityId);
            ParentIds.Remove(entityId);
            if (entity.TemplateId != null) {
                Variants.Remove(entity.TemplateId);
            }
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

        public IEntity<T> CreateVariant(T entityData) {
            var variant = TemplatesIo.CreateVariantInstanceOf(entityData);
            return Add(variant);
        }

        public void SaveChanges(T entityData) {
            TemplatesIo.SaveAsTemplate(entityData);
            Update(entityData);
        }

    }

}