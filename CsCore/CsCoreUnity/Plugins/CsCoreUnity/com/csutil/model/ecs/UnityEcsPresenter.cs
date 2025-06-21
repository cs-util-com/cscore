using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary> The cache of old states that where already applied to the presenters </summary>
        private readonly Dictionary<string, T> _oldStates = new Dictionary<string, T>();

        /// <summary> Debouncing the ui/view/unity presenter updates is important to not
        /// hand over half updated states and in general to not have to update the view too often </summary>
        private readonly Func<IEntity<T>, Task> _updateGoDebounced;

        protected UnityEcsPresenter(int delayInMs = 50) {
            Func<IEntity<T>, Task> t = (latestEntityState) => {
                UpdateGoFor(latestEntityState);
                return Task.CompletedTask;
            };
            if (delayInMs <= 0) {
                _updateGoDebounced = t;
            } else {
                _updateGoDebounced = t.AsThrottledDebounceV2(delayInMs, skipFirstEvent: true);
            }
        }

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
            _oldStates.Clear();
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
            var originalStackTrace = StackTraceV2.NewStackTrace();
            TaskV2.Run(async () => {
                // Delay initial creation and updates of presenters so that the model is already in a stable state when the presenter is informed
                await TaskV2.Delay(100);
                MainThread.Invoke(() => {
                    if (!this.IsAlive()) { return; }
                    // If the entity is no longer alive after the main thread switch, ignore the update (except for remove events):
                    var isUpdateInMainThreadStillPossible = iEntity.IsAlive() || type == EntityComponentSystem<T>.UpdateType.Remove;
                    if (isUpdateInMainThreadStillPossible) {
                        try {
                            HandleEntityUpdate(iEntity, type, oldstate);
                        } catch (Exception e) {
                            throw e.WithAddedOriginalStackTrace(originalStackTrace);
                        }
                    }
                });
            }).LogOnError();
        }

        private void HandleEntityUpdate(IEntity<T> iEntity, EntityComponentSystem<T>.UpdateType type, T oldstate) {
            switch (type) {
                case EntityComponentSystem<T>.UpdateType.Add:
                    CreateGoFor(iEntity);
                    _oldStates.Add(iEntity.Id, iEntity.Data);
                    break;
                case EntityComponentSystem<T>.UpdateType.Remove:
                    RemoveGoFor(iEntity, oldstate);
                    _oldStates.Remove(iEntity.Id);
                    break;
                case EntityComponentSystem<T>.UpdateType.Update:
                case EntityComponentSystem<T>.UpdateType.TemplateUpdate:
                    _updateGoDebounced(iEntity);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void CreateGoFor(IEntity<T> iEntity) {
            if (_entityViews.ContainsKey(iEntity.Id)) { return; }
            GameObject go = NewGameObjectFor(iEntity);
            AssertV3.IsNull(go.GetComponent<EntityView>(), "go.GetComponent<EntityView>()");
            go.AddComponent<EntityView>().Init(iEntity);
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
                OnComponentAdded(iEntity, x.Value, targetParentGo: go);
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

        private void UpdateGoFor(IEntity<T> iEntity) {

            T newState = iEntity.Data;
            if (!_oldStates.TryGetValue(newState.Id, out T oldState)) {
                throw Log.e($"Failed to find old state for entity={iEntity}");
            }
            _oldStates[newState.Id] = newState;

            var go = _entityViews[iEntity.Id];
            if (go.IsDestroyed()) {
                throw Log.e($"The entity view for {iEntity.GetFullEcsPathString()} was destroyed");
            }
            go.SetActiveV2(iEntity.IsActiveSelf());
            if (oldState.Name != "" + newState) {
                go.name = "" + iEntity;
            }
            var newLocalPose3d = newState.LocalPose.ToPose();
            AssertV3.IsNotNull(newLocalPose3d, "newLocalPose3d");
            if (oldState.ParentId != newState.ParentId) {
                if (newState.ParentId != null) {
                    OnChangeParent(newState, go, newLocalPose3d);
                } else {
                    OnDetachFromParent(go, newLocalPose3d);
                }
            }
            if (oldState.LocalPose != newState.LocalPose) {
                OnPoseUpdate(iEntity, go, newLocalPose3d);
            }
            var newIsActiveState = newState.IsActiveSelf();
            if (oldState.IsActiveSelf() != newIsActiveState) {
                OnToggleActiveState(go, newIsActiveState);
            }
            var oldComps = oldState.Components;
            newState.Components.CalcEntryChangesToOldStateV4<IReadOnlyDictionary<string, IComponentData>, string, IComponentData>(ref oldComps,
                (key, added) => OnComponentAdded(iEntity, added, targetParentGo: go),
                (key, old, updated) => OnComponentUpdated(iEntity, key, oldState.Components[key], updated, targetParentGo: go),
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

        protected virtual void OnComponentAdded(IEntity<T> iEntity, IComponentData added, GameObject targetParentGo) {
            var createdComponent = AddComponentTo(targetParentGo, added, iEntity);
            if (createdComponent == null) {
                //Log.d($"AddComponentTo returned NULL for component={added.Value} and targetParentGo={targetParentGo}", targetParentGo);
            } else {
                createdComponent.ComponentId = added.GetId();
                if (createdComponent is Behaviour mono) {
                    mono.enabled = true; // Force to trigger onEnable once (often needed by presenters to init some values)
                    if (mono.enabled != added.IsActive) {
                        mono.enabled = added.IsActive;
                    }
                }
                createdComponent.OnUpdateUnityComponent(iEntity, default, added);
            }
        }

        protected abstract IComponentPresenter<T> AddComponentTo(GameObject targetGo, IComponentData componentModel, IEntity<T> iEntity);

        protected virtual void OnComponentRemoved(IEntity<T> iEntity, string deleted, GameObject targetParentGo) {
            var allPresenters = GetComponentPresenters(iEntity);
            if (allPresenters.Any(x => x.ComponentId == deleted)) {
                if (TryGetComponentPresenter(iEntity, deleted, out var compPresenter)) {
                    if (compPresenter is IEcsComponentDestroyHandler handler && !handler.OnDestroyRequest()) {
                        // Do not dispose the comp presenter, since the handle logic should already have handled the
                        // remove request (e.g. by disposing the presenter or just resetting it)
                    } else {
                        compPresenter.DisposeV2();
                    }
                }
            } else {
                Log.d($"No component found for the deleted entity component with id={deleted} in entity={iEntity}. "
                    + $"allPresenters={allPresenters.ToStringV2(x => "\n" + x)}", targetParentGo);
            }
        }

        private bool TryGetComponentPresenter(IEntity<T> iEntity, string componentId, out IComponentPresenter<T> componentPresenter) {
            try {
                var componentPresenters = GetComponentPresenters(iEntity);
                componentPresenter = componentPresenters.SingleOrDefault(x => x.ComponentId == componentId);
                return componentPresenter != null;
            } catch (Exception e) {
                var entityView = _entityViews[iEntity.Id];
                Log.e($"Failed to find exactly 1 component with id={componentId} in entity={iEntity}", e, entityView);
                componentPresenter = null;
                return false;
            }
        }

        private IEnumerable<IComponentPresenter<T>> GetComponentPresenters(IEntity<T> iEntity) {
            return _entityViews[iEntity.Id].GetComponentsInOwnEcsPresenterChildren<IComponentPresenter<T>>(iEntity, includeInactive: true);
        }

        protected virtual void OnComponentUpdated(IEntity<T> iEntity, string key, IComponentData oldState, IComponentData updatedState, GameObject targetParentGo) {
            if (TryGetComponentPresenter(iEntity, key, out var compView)) {
                if (compView is Behaviour mono) {
                    mono.enabled = updatedState.IsActive;
                }
                compView.OnUpdateUnityComponent(iEntity, oldState, updatedState);
            }
        }

    }

}