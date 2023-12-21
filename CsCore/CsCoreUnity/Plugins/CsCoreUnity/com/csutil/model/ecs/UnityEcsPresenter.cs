using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.model.ecs {

    public abstract class UnityEcsPresenter<T> : Presenter<EntityComponentSystem<T>> where T : IEntityData {

        /// <summary> The root of the Unity view (the Scene graph composed of Unity GameObjects that visualize the ECS model </summary>
        public GameObject targetView { get; set; }

        private Dictionary<string, GameObject> _entityViews = new Dictionary<string, GameObject>();
        private Dictionary<string, IComponentPresenter<T>> _componentViews = new Dictionary<string, IComponentPresenter<T>>();
        public IReadOnlyDictionary<string, GameObject> EntityViews => _entityViews;
        public IReadOnlyDictionary<string, IComponentPresenter<T>> ComponentViews => _componentViews;

        public virtual Task OnLoad(EntityComponentSystem<T> model) {
            var entitiesInRoot = model.Entities.Values.Filter(x => x.ParentId == null);
            AddViewsForEntityModelsRecursively(entitiesInRoot);
            model.OnIEntityUpdated += OnEntityUpdated;
            return Task.FromResult(true);
        }

        /// <summary> Traverses the entity tree recursively and creates the Unity GameObjects for each entity </summary>
        private void AddViewsForEntityModelsRecursively(IEnumerable<IEntity<T>> entities) {
            foreach (IEntity<T> entity in entities) {
                OnEntityUpdated(entity, EntityComponentSystem<T>.UpdateType.Add, default, entity.Data);
                var children = entity.GetChildren();
                if (children != null) { AddViewsForEntityModelsRecursively(children); }
            }
        }

        private void OnEntityUpdated(IEntity<T> iEntity, EntityComponentSystem<T>.UpdateType type, T oldstate, T newstate) {
            MainThread.Invoke(() => { HandleEntityUpdate(iEntity, type, oldstate, newstate); });
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
            var go = NewGameObjectFor(iEntity);
            go.name = iEntity.Name;
            _entityViews.Add(iEntity.Id, go);
            if (iEntity.ParentId != null) {
                _entityViews[iEntity.ParentId].AddChild(go);
            } else {
                targetView.AddChild(go);
            }
            iEntity.LocalPose().ApplyTo(go.transform);
            go.SetActive(iEntity.IsActive);
            foreach (var x in iEntity.Components) {
                onCompAdded(iEntity, x, targetParentGo: go);
            }
        }

        protected abstract GameObject NewGameObjectFor(IEntity<T> iEntity);

        private void RemoveGoFor(IEntity<T> iEntity, T removedEntity) {
            _entityViews[iEntity.Id].Destroy();
            _entityViews.Remove(iEntity.Id);
        }

        private void UpdateGoFor(IEntity<T> iEntity, T oldState, T newState) {
            var go = _entityViews[iEntity.Id];
            if (oldState.Name != newState.Name) {
                go.name = iEntity.Name;
            }
            if (oldState.ParentId != newState.ParentId) {
                if (newState.ParentId != null) {
                    OnChangeParent(newState, go);
                } else {
                    OnDetachFromParent(go);
                }
            }
            if (oldState.LocalPose != newState.LocalPose) {
                OnPoseUpdate(iEntity, go, iEntity.LocalPose());
            }
            var newIsActiveState = newState.IsActive;
            if (oldState.IsActive != newIsActiveState) {
                OnToggleActiveState(go, newIsActiveState);
            }
            var oldComps = oldState.Components;
            newState.Components.CalcEntryChangesToOldStateV2<IReadOnlyDictionary<string, IComponentData>, string, IComponentData>(ref oldComps,
                added => onCompAdded(iEntity, added, targetParentGo: go),
                updated => OnComponentUpdated(iEntity, oldState.Components[updated.Key], updated, targetParentGo: go),
                deleted => OnCompentRemoved(iEntity, deleted, targetParentGo: go)
            );
        }
        
        protected virtual void OnToggleActiveState(GameObject go, bool newIsActiveState) { go.SetActive(newIsActiveState); }

        protected virtual void OnChangeParent(T newState, GameObject go) { go.transform.SetParent(_entityViews[newState.ParentId].transform); }
        
        protected virtual void OnDetachFromParent(GameObject go) { go.transform.SetParent(targetView.transform); }

        protected virtual void OnPoseUpdate(IEntity<T> iEntity, GameObject go, Pose3d newLocalPose) { newLocalPose.ApplyTo(go.transform); }

        private void onCompAdded(IEntity<T> iEntity, KeyValuePair<string, IComponentData> added, GameObject targetParentGo) {
            var createdComponent = AddComponentTo(targetParentGo, added.Value, iEntity);
            if (createdComponent == null) {
                throw Log.e($"AddComponentTo returned NULL for component={added.Value} and targetParentGo={targetParentGo}", targetParentGo);
            }
            _componentViews.Add(added.Key, createdComponent);
            createdComponent.OnUpdateUnityComponent(iEntity, default, added.Value);
        }

        protected abstract IComponentPresenter<T> AddComponentTo(GameObject targetGo, IComponentData componentModel, IEntity<T> iEntity);

        protected virtual void OnCompentRemoved(IEntity<T> iEntity, string deleted, GameObject targetParentGo) {
            _componentViews[deleted].DisposeV2();
            _componentViews.Remove(deleted);
        }

        protected virtual void OnComponentUpdated(IEntity<T> iEntity, IComponentData oldState, KeyValuePair<string, IComponentData> updatedState, GameObject targetParentGo) {
            var compView = _componentViews[updatedState.Key];
            compView.OnUpdateUnityComponent(iEntity, oldState, updatedState.Value);
        }

    }

}