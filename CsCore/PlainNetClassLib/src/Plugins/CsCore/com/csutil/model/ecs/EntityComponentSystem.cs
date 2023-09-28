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
            public IReadOnlyDictionary<string, IComponentData> Components => Data.Components;
            public string ParentId => Data.ParentId;
            public IReadOnlyList<string> ChildrenIds => Data.ChildrenIds;

            public override string ToString() { return Data.ToString(); }

        }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        private readonly TemplatesIO<T> TemplatesIo;

        private readonly Dictionary<string, IEntity<T>> Entities = new Dictionary<string, IEntity<T>>();

        /// <summary> A lookup from templateId to all entityIds that are variants of that template </summary>
        private readonly Dictionary<string, HashSet<string>> Variants = new Dictionary<string, HashSet<string>>();

        public IReadOnlyDictionary<string, IEntity<T>> AllEntities => Entities;

        /// <summary> If set to true the T class used in IEntity<T> must be immutable in all fields </summary>
        public readonly bool IsModelImmutable;

        /// <summary> Triggered when the entity is directly or indirectly changed (e.g. when a template entity is changed).
        /// Will path the IEntity wrapper, the old and the new data </summary>
        public event IEntityUpdateListener OnIEntityUpdated;

        /// <summary>
        /// 
        /// </summary>
        public delegate void IEntityUpdateListener(IEntity<T> iEntityWrapper, UpdateType type, T oldState, T newState);

        public enum UpdateType { Add, Remove, Update }

        public EntityComponentSystem(TemplatesIO<T> templatesIo, bool isModelImmutable) {
            TemplatesIo = templatesIo;
            IsModelImmutable = isModelImmutable;
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            TemplatesIo.DisposeV2();
            Entities.Clear();
            Variants.Clear();
            OnIEntityUpdated = null;
            IsDisposed = DisposeState.Disposed;
        }

        public IEntity<T> Add(T entityData) {
            // First check if the entity already exists:
            if (Entities.TryGetValue(entityData.Id, out var existingEntity)) {
                throw new InvalidOperationException("Entity already exists with id '" + entityData.Id + "' old=" + existingEntity.Data + " new=" + entityData);
            }
            return AddEntity(new Entity() { Data = entityData, Ecs = this });
        }

        private IEntity<T> AddEntity(Entity entity) {
            var entityId = entity.Id;
            entityId.ThrowErrorIfNullOrEmpty("entityData.Id");
            var hasOldEntity = Entities.TryGetValue(entityId, out var oldEntity);
            T oldEntityData = hasOldEntity ? oldEntity.Data : default;
            Entities[entityId] = entity;
            UpdateVariantsLookup(entity.Data);
            OnIEntityUpdated?.Invoke(entity, UpdateType.Add, oldEntityData, entity.Data);
            return entity;
        }

        private void UpdateVariantsLookup(T entity) {
            if (entity.TemplateId != null) {
                Variants.AddToValues(entity.TemplateId, entity.Id);
            }
        }

        public void Update(T updatedEntityData) {
            var entityId = updatedEntityData.Id;
            var entity = (Entity)Entities[entityId];

            var oldEntryData = entity.Data;

            // e.g. if the entries are mutable this will mostly be true:
            var oldAndNewSameEntry = ReferenceEquals(oldEntryData, updatedEntityData);
            if (IsModelImmutable && oldAndNewSameEntry) {
                return; // only for immutable data it is now clear that no update is required
            }
            if (!oldAndNewSameEntry) {
                // Compute json diff to know if the entry really changed and if not skip informing all variants about the change:
                // This can happen eg if the variant overwrites the field that was just changed in the template
                if (!TemplatesIo.HasChanges(oldEntryData, updatedEntityData)) { return; }
            }
            entity.Data = updatedEntityData;
            // At this point in the update method it is known that the entity really changed and 
            OnIEntityUpdated?.Invoke(entity, UpdateType.Update, oldEntryData, updatedEntityData);

            // If the entry is a template for other entries, then all variants need to be updated:
            if (Variants.TryGetValue(updatedEntityData.Id, out var variantIds)) {
                foreach (var variantId in variantIds) {
                    var newVariantState = TemplatesIo.RecreateVariantInstance(variantId);
                    Update(newVariantState);
                }
            }
        }

        public async Task LoadSceneGraphFromDisk() {
            await TemplatesIo.LoadAllTemplateFilesIntoMemory();
            foreach (var entityId in TemplatesIo.GetAllEntityIds()) {
                Add(TemplatesIo.ComposeEntityInstance(entityId));
            }
        }

        public IEntity<T> GetEntity(string entityId) {
            return Entities[entityId];
        }

        public void Destroy(string entityId) {
            var entity = Entities[entityId] as Entity;
            OnIEntityUpdated?.Invoke(entity, UpdateType.Remove, entity.Data, default);
            Entities.Remove(entityId);
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

        public IEntity<T> CreateVariant(T entityData, Dictionary<string, string> newIdsLookup) {
            var variant = TemplatesIo.CreateVariantInstanceOf(entityData, newIdsLookup);
            return Add(variant);
        }

        public void SaveChanges(T entityData) {
            TemplatesIo.SaveChanges(entityData);
            Update(entityData);
        }

    }

}