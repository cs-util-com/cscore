using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.model.ecs {

    public abstract class UnityEcsPresenter<T> : Presenter<EntityComponentSystem<T>>, IDisposableV2 where T : IEntityData {

        /// <summary> The root of the Unity view (the Scene graph composed of Unity GameObjects that visualize the ECS model </summary>
        public GameObject targetView { get; set; }

        private Dictionary<string, GameObject> _entityViews = new Dictionary<string, GameObject>();
        public IReadOnlyDictionary<string, GameObject> EntityViews => _entityViews;

        public DisposeState IsDisposed { get; private set; }

        public virtual Task OnLoad(EntityComponentSystem<T> model) {
            targetView.ThrowErrorIfNullOrDestroyed("Root GameObject of the ECS presenter");
            var entitiesInRoot = model.Entities.Values.Filter(x => x.ParentId == null);
            AddViewsForEntityModelsRecursively(entitiesInRoot);
            model.OnIEntityUpdated += OnEntityUpdated;
            return Task.CompletedTask;
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            OnDispose();
            foreach (var child in _entityViews.Values) {
                child.Destroy();
            }
            _entityViews.Clear();
            _entityViews = null;
            IsDisposed = DisposeState.Disposed;
        }

        protected virtual void OnDispose() { }

        /// <summary> Traverses the entity tree recursively and creates the Unity GameObjects for each entity </summary>
        private void AddViewsForEntityModelsRecursively(IEnumerable<IEntity<T>> entities) {
            foreach (IEntity<T> entity in entities) {
                OnEntityUpdated(entity, EntityComponentSystem<T>.UpdateType.Add, default, entity.Data);
                var children = entity.GetChildren();
                if (children != null) { AddViewsForEntityModelsRecursively(children); }
            }
        }

        private void OnEntityUpdated(IEntity<T> iEntity, EntityComponentSystem<T>.UpdateType type, T oldstate, T newstate) {
            AssertV3.IsTrue(iEntity.IsAlive(), () => "Entity is not alive");
            MainThread.Invoke(() => {
                // If the entity is no longer alive after the main thread switch, ignore the update (except for remove events):
                var isUpdateInMainThreadStillPossible = iEntity.IsAlive() || type == EntityComponentSystem<T>.UpdateType.Remove;
                if (isUpdateInMainThreadStillPossible) {
                    HandleEntityUpdate(iEntity, type, oldstate, newstate);
                }
            });
        }

        private void HandleEntityUpdate(IEntity<T> iEntity, EntityComponentSystem<T>.UpdateType type, T oldstate, T newstate) {
            switch (type) {
                case EntityComponentSystem<T>.UpdateType.Add:
                    CreateGoFor(iEntity);
                    break;
                case EntityComponentSystem<T>.UpdateType.Remove:
                    RemoveGoFor(iEntity, oldstate);
                    break;
                case EntityComponentSystem<T>.UpdateType.Update:
                    UpdateGoFor(iEntity, oldstate, newstate);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void CreateGoFor(IEntity<T> iEntity) {
            if (_entityViews.ContainsKey(iEntity.Id)) { return; }
            GameObject go = NewGameObjectFor(iEntity);
            _entityViews.Add(iEntity.Id, go);
            go.name = "" + iEntity;
            if (iEntity.ParentId != null) {
                var parent = iEntity.GetParent();
                if (!_entityViews.ContainsKey(parent.Id)) {
                    CreateGoFor(parent);
                }
                _entityViews[iEntity.ParentId].AddChild(go);
            } else {
                targetView.AddChild(go);
            }
            iEntity.LocalPose().ApplyTo(go.transform);
            go.SetActive(iEntity.IsActiveSelf());
            foreach (var x in iEntity.Components) {
                OnComponentAdded(iEntity, x, targetParentGo: go);
            }
        }

        protected abstract GameObject NewGameObjectFor(IEntity<T> iEntity);

        private void RemoveGoFor(IEntity<T> iEntity, T removedEntity) {
            if (_entityViews.ContainsKey(iEntity.Id)) {
                _entityViews[iEntity.Id].Destroy();
                if (!_entityViews.Remove(iEntity.Id)) {
                    Log.e($"Failed to remove GO view/presenter of entity={iEntity}");
                }
            } else {
                Log.e($"Failed to remove GO view/presenter of entity={iEntity}");
            }
        }

        private void UpdateGoFor(IEntity<T> iEntity, T oldState, T newState) {
            var go = _entityViews[iEntity.Id];
            if (go.IsDestroyed()) {
                throw Log.e($"The entity view for {iEntity.GetFullEcsPathString()} was destroyed");
            }
            go.SetActiveV2(iEntity.IsActiveSelf());
            if (oldState.Name != "" + newState) {
                go.name = "" + iEntity;
            }
            if (oldState.ParentId != newState.ParentId) {
                if (newState.ParentId != null) {
                    OnChangeParent(newState, go, iEntity.LocalPose());
                } else {
                    OnDetachFromParent(go, iEntity.LocalPose());
                }
            }
            if (oldState.LocalPose != newState.LocalPose) {
                OnPoseUpdate(iEntity, go, iEntity.LocalPose());
            }
            var newIsActiveState = newState.IsActiveSelf();
            if (oldState.IsActiveSelf() != newIsActiveState) {
                OnToggleActiveState(go, newIsActiveState);
            }
            var oldComps = oldState.Components;
            newState.Components.CalcEntryChangesToOldStateV2<IReadOnlyDictionary<string, IComponentData>, string, IComponentData>(ref oldComps,
                added => OnComponentAdded(iEntity, added, targetParentGo: go),
                (_, updated) => OnComponentUpdated(iEntity, oldState.Components[updated.Key], updated, targetParentGo: go),
                deleted => OnComponentRemoved(iEntity, deleted, targetParentGo: go)
            );
        }

        protected virtual void OnToggleActiveState(GameObject go, bool newIsActiveState) {
            go.SetActive(newIsActiveState);
        }

        protected virtual void OnChangeParent(T newState, GameObject go, Pose3d newLocalPose) {
            go.transform.SetParent(_entityViews[newState.ParentId].transform);
            newLocalPose.ApplyTo(go.transform);
        }

        protected virtual void OnDetachFromParent(GameObject go, Pose3d newLocalPose) {
            go.transform.SetParent(targetView.transform);
            newLocalPose.ApplyTo(go.transform);
        }

        protected virtual void OnPoseUpdate(IEntity<T> iEntity, GameObject go, Pose3d newLocalPose) {
            newLocalPose.ApplyTo(go.transform);
        }

        protected virtual void OnComponentAdded(IEntity<T> iEntity, KeyValuePair<string, IComponentData> added, GameObject targetParentGo) {
            var createdComponent = AddComponentTo(targetParentGo, added.Value, iEntity);
            if (createdComponent == null) {
                Log.d($"AddComponentTo returned NULL for component={added.Value} and targetParentGo={targetParentGo}", targetParentGo);
            } else {
                createdComponent.ComponentId = added.Value.GetId();
                if (createdComponent is Behaviour mono) {
                    mono.enabled = true; // Force to trigger onEnable once (often needed by presenters to init some values)
                    if (mono.enabled != added.Value.IsActive) {
                        mono.enabled = added.Value.IsActive;
                    }
                }
                createdComponent.OnUpdateUnityComponent(iEntity, default, added.Value);
            }
        }

        protected abstract IComponentPresenter<T> AddComponentTo(GameObject targetGo, IComponentData componentModel, IEntity<T> iEntity);

        protected virtual void OnComponentRemoved(IEntity<T> iEntity, string deleted, GameObject targetParentGo) {
            var allPresenters = GetComponentPresenters(iEntity);
            if (allPresenters.Any(x => x.ComponentId == deleted)) {
                GetComponentPresenter(iEntity, deleted).DisposeV2();
            } else {
                Log.d($"No component found for the deleted entity component with id={deleted} in entity={iEntity}. "
                    + $"allPresenters={allPresenters.ToStringV2(x => "\n" + x)}", targetParentGo);
            }
        }

        private IComponentPresenter<T> GetComponentPresenter(IEntity<T> iEntity, string componentId) {
            try {
                var componentPresenters = GetComponentPresenters(iEntity);
                return componentPresenters.Single(x => x.ComponentId == componentId);
            } catch (Exception e) {
                var entityView = _entityViews[iEntity.Id];
                Log.e($"Failed to find exactly 1 component with id={componentId} in entity={iEntity}", e, entityView);
                throw;
            }
        }

        private IComponentPresenter<T>[] GetComponentPresenters(IEntity<T> iEntity) {
            return _entityViews[iEntity.Id].GetComponentsInChildren<IComponentPresenter<T>>(true);
        }

        protected virtual void OnComponentUpdated(IEntity<T> iEntity, IComponentData oldState, KeyValuePair<string, IComponentData> updatedState, GameObject targetParentGo) {
            var compView = GetComponentPresenter(iEntity, updatedState.Key);
            if (compView is Behaviour mono) {
                mono.enabled = updatedState.Value.IsActive;
            }
            compView.OnUpdateUnityComponent(iEntity, oldState, updatedState.Value);
        }

    }

}