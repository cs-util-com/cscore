using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using com.csutil.model.immutable;
using Newtonsoft.Json;

namespace com.csutil.model.ecs {

    public class EntityComponentSystem<T> : IDisposableV2 where T : IEntityData {

        private class Entity : IEntity<T> {

            public string GetId() { return Data.GetId(); }
            public T Data { get; set; }
            public EntityComponentSystem<T> Ecs { get; private set; }

            [JsonConstructor]
            public Entity(T data, EntityComponentSystem<T> ecs) {
                Data = data;
                Ecs = ecs;
            }

            public Action<IEntity<T>> OnCreate { get; set; }
            public Action<T, IEntity<T>, UpdateType> OnUpdate { get; set; }

            public string Id => Data.Id;
            public string Name => Data.Name;
            public string TemplateId => Data.TemplateId;
            public Matrix4x4? LocalPose => Data.LocalPose;
            public IReadOnlyDictionary<string, IComponentData> Components => Data.Components;
            public string ParentId => Data.ParentId;
            public IReadOnlyList<string> ChildrenIds => Data.ChildrenIds;
            public bool IsActive => Data.IsActive;

            public override string ToString() {
                var str = Data.ToString();
                if (!this.IsAlive()) { str = $"(DESTROYED) {str}"; }
                return str;
            }

            public virtual void Dispose() {
                IsDisposed = DisposeState.DisposingStarted;
                if (Ecs != null) { Ecs.Destroy(this).LogOnError(); }
                Ecs = null;
                IsDisposed = DisposeState.Disposed;
            }

            public DisposeState IsDisposed { get; set; } = DisposeState.Active;

        }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        private readonly TemplatesIO<T> TemplatesIo;

        private readonly Dictionary<string, IEntity<T>> _entities = new Dictionary<string, IEntity<T>>();

        /// <summary> A lookup from templateId to all entityIds that are variants of that template </summary>
        private readonly Dictionary<string, HashSet<string>> _variants = new Dictionary<string, HashSet<string>>();

        public IReadOnlyDictionary<string, IEntity<T>> Entities => _entities;
        public IReadOnlyCollection<string> TemplateIds => _variants.Keys;
        public IEnumerable<IEntity<T>> Templates => TemplateIds.Map(GetEntity);

        /// <summary> If set to true the T class used in IEntity<T> must be immutable in all fields </summary>
        public readonly bool IsModelImmutable;

        /// <summary> Triggered when the entity is created, removed or directly/indirectly changed (e.g. when a template entity is changed).
        /// Will pass the IEntity wrapper, the old and the new data </summary>
        public event IEntityUpdateListener OnIEntityUpdated;

        public delegate void IEntityUpdateListener(IEntity<T> iEntity, UpdateType updateType, T oldState, T newState);

        public enum UpdateType {
            Add,
            Remove,
            Update,
            /// <summary> Used when the entity was updated by the ECS because the template of the entity was updated </summary>
            TemplateUpdate
        }

        public EntityComponentSystem(TemplatesIO<T> templatesIo, bool isModelImmutable) {
            templatesIo.ThrowErrorIfNull("templatesIo");
            TemplatesIo = templatesIo;
            IsModelImmutable = isModelImmutable;
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            OnDispose();
            _variants.Clear();
            TemplatesIo.DisposeV2();
            foreach (IEntity<T> entity in _entities.Values) {
                ((Entity)entity).IsDisposed = DisposeState.Disposed;
            }
            _entities.Clear();
            OnIEntityUpdated = null;
            IsDisposed = DisposeState.Disposed;
        }

        protected virtual void OnDispose() { }

        public IEntity<T> Add(T entityData, bool informEntityAddedListeners = true) {
            this.ThrowErrorIfDisposed();
            // First check if the entity already exists:
            if (_entities.TryGetValue(entityData.Id, out var existingEntity)) {
                throw new InvalidOperationException("Entity already exists with id '" + entityData.Id + "' old=" + existingEntity.Data + " new=" + entityData);
            }
            return AddEntity(new Entity(entityData, this), informEntityAddedListeners);
        }

        private IEntity<T> AddEntity(Entity entity, bool informEntityAddedListeners) {
            var entityId = entity.Id;
            entityId.ThrowErrorIfNullOrEmpty("entityData.Id");
            var hasOldEntity = _entities.TryGetValue(entityId, out var oldEntity);
            T oldEntityData = hasOldEntity ? oldEntity.Data : default;
            _entities[entityId] = entity;
            UpdateVariantsLookup(entity.Data);
            if (informEntityAddedListeners) {
                InformEntityAddedListeners(entity, oldEntityData);
            }
            return entity;
        }

        internal void InformEntityAddedListeners(IEntity<T> entity, T oldEntityData) {
            AssertV3.IsNull(oldEntityData, "oldEntityData");
            OnIEntityUpdated?.Invoke(entity, UpdateType.Add, oldEntityData, entity.Data);
            entity.OnCreate?.Invoke(entity);
            if (entity.Components != null) {
                foreach (var comp in entity.Components.Values) {
                    if (comp is IAddedToEntityListener<T> l) {
                        l.OnAddedToEntity(entity);
                    }
                }
            }
        }

        private void UpdateVariantsLookup(T entity) {
            if (entity.TemplateId != null) {
                _variants.AddToValues(entity.TemplateId, entity.Id);
            }
        }

        public Task Update(T entityData, UpdateType updateType = UpdateType.Update) {
            this.ThrowErrorIfDisposed();
            var t = TemplatesIo?.SaveChanges(entityData);
            InternalUpdate(entityData, updateType);
            return t;
        }

        private void InternalUpdate(T updatedEntityData, UpdateType updateType) {
            var entityId = updatedEntityData.Id;
            var entity = (Entity)_entities[entityId];
            entity.ThrowErrorIfDisposed();

            var oldEntryData = entity.Data;

            // e.g. if the entries are mutable this will mostly be true:
            var wasModified = StateCompare.WasModified(oldEntryData, updatedEntityData);
            if (IsModelImmutable && !wasModified) {
                return; // only for immutable data it is now clear that no update is required
            }
            if (wasModified) {
                // Compute json diff to know if the entry really changed and if not skip informing all variants about the change:
                // This can happen eg if the variant overwrites the field that was just changed in the template
                if (!TemplatesIo.HasChanges(oldEntryData, updatedEntityData)) {
                    // Even if entity.Data and updatedEntityData contain the same content, the data must still be set to the new reference:
                    entity.Data = updatedEntityData;
                    return;
                }
            }
            entity.Data = updatedEntityData;
            UpdateVariantsLookup(entity.Data);
            // At this point in the update method it is known that the entity really changed  
            OnIEntityUpdated?.Invoke(entity, updateType, oldEntryData, updatedEntityData);
            entity.OnUpdate?.Invoke(oldEntryData, entity, updateType);
            // Also update all components of that entity that implement IEntityUpdateListener:
            if (updatedEntityData.Components != null) {
                foreach (var comp in updatedEntityData.Components.Values) {
                    if (comp is IParentEntityUpdateListener<T> l) {
                        l.OnParentEntityUpdate(oldEntryData, entity);
                    }
                }
            }

            // If the entry is a template for other entries, then all variants need to be updated:
            if (_variants.TryGetValue(updatedEntityData.Id, out var variantIds)) {
                UpdateVariantsWhenTemplateChanges(variantIds);
            }
        }

        /// <summary> Returns direct variants of an entity if the entity is a template </summary>
        public virtual bool TryGetVariants(string templateId, out IEnumerable<IEntity<T>> variantIds) {
            this.ThrowErrorIfDisposed();
            if (_variants.TryGetValue(templateId, out var variants)) {
                variantIds = variants.Map(id => _entities[id]);
                return true;
            } else {
                variantIds = null;
                return false;
            }
        }

        protected virtual void UpdateVariantsWhenTemplateChanges(HashSet<string> variantIds) {
            foreach (var variantId in variantIds) {
                var newVariantState = TemplatesIo.RecreateVariantInstance(variantId);
                InternalUpdate(newVariantState, UpdateType.TemplateUpdate);
            }
        }

        [Obsolete("Use LoadAllEntitiesFromDisk() instead", true)]
        public async Task LoadSceneGraphFromDisk() {
            this.ThrowErrorIfDisposed();
            await TemplatesIo.LoadAllTemplateFilesIntoMemory();
            foreach (var entityId in TemplatesIo.GetAllEntityIds()) {
                Add(TemplatesIo.ComposeEntityInstance(entityId));
            }
        }

        public Task<List<T>> LoadAllEntitiesFromDisk() {
            this.ThrowErrorIfDisposed();
            return TemplatesIo.LoadAllEntitiesFromDisk();
        }

        public IEntity<T> GetEntity(string entityId) {
            this.ThrowErrorIfDisposed();
            return _entities[entityId];
        }

        public bool TryGetEntity(string entityId, out IEntity<T> entity) {
            this.ThrowErrorIfDisposed();
            return _entities.TryGetValue(entityId, out entity);
        }

        public Task Destroy(IEntity<T> entityToDestroy) {
            this.ThrowErrorIfDisposed();
            if (!entityToDestroy.IsAlive()) { return Task.CompletedTask; }
            return Destroy(entityToDestroy.Id);
        }

        private Task Destroy(string entityId) {
            var entity = _entities[entityId] as Entity;
            _entities.Remove(entityId);
            if (_variants.ContainsKey(entityId)) {
                _variants.Remove(entityId);
            }
            if (entity.TemplateId != null) {
                if (!_variants.RemoveFromValues(entity.TemplateId, entityId)) {
                    throw new InvalidOperationException("Failed to remove variant id from template variants");
                }
            }
            var entityData = entity.Data;
            OnIEntityUpdated?.Invoke(entity, UpdateType.Remove, entityData, default);
            entity.DisposeV2();
            DisposeEntityData(entityData);
            return TemplatesIo.Delete(entityId);
        }

        private static void DisposeEntityData(T entityData) {
            if (entityData.Components != null) {
                foreach (var comp in entityData.Components) {
                    DisposeIfPossible(comp);
                }
            }
            DisposeIfPossible(entityData);
        }

        private static void DisposeIfPossible(object x) {
            if (x is IDisposableV2 d2) {
                d2.DisposeV2();
            } else if (x is IDisposable d1) {
                d1.Dispose();
            }
        }

        /// <summary> Creates a variant instance of the entity (but not its children entities) </summary>
        /// <param name="sourceEntity"> The entity that will become the template of the created variant </param>
        /// <param name="newIdsLookup"> the ids that should be used for the variant </param>
        /// <param name="informEntityAddedListeners"> Should be set to false if multiple entities are created at once for the variant </param>
        /// <returns> The created variant entity </returns>
        public IEntity<T> CreateVariant(T sourceEntity, IReadOnlyDictionary<string, string> newIdsLookup, bool informEntityAddedListeners, bool autoSaveIfNeeded = false) {
            this.ThrowErrorIfDisposed();
            return Add(TemplatesIo.CreateVariantInstanceOf(sourceEntity, newIdsLookup, autoSaveIfNeeded), informEntityAddedListeners);
        }

    }

}