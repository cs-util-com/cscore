using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.model.ecs {

    public abstract class UnityEcsPresenter<T> : Presenter<EntityComponentSystem<T>> where T : IEntityData {

        /// <summary> The root of the Unity view (the Scene graph composed of Unity GameObjects that visualize the ECS model </summary>
        public GameObject targetView { get; set; }

        private Dictionary<string, GameObject> _entityViews = new Dictionary<string, GameObject>();
        private Dictionary<string, IComponentPresenter> _componentViews = new Dictionary<string, IComponentPresenter>();

        public Task OnLoad(EntityComponentSystem<T> model) {

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
            switch (type) {
                case EntityComponentSystem<T>.UpdateType.Add:
                    CreateGoFor(iEntity, newstate);
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

        private void CreateGoFor(IEntity<T> iEntity, T addedEntity) {
            var go = new GameObject(iEntity.Name);
            _entityViews.Add(iEntity.Id, go);
            if (iEntity.ParentId != null) {
                _entityViews[iEntity.ParentId].AddChild(go);
            } else {
                targetView.AddChild(go);
            }
            iEntity.LocalPose().ApplyTo(go.transform);
            go.SetActive(addedEntity.IsActive);
            foreach (var x in addedEntity.Components) {
                onCompAdded(iEntity, x, targetParentGo: go);
            }
        }

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
                    go.transform.SetParent(_entityViews[newState.ParentId].transform);
                } else {
                    go.transform.SetParent(targetView.transform);
                }
            }
            if (oldState.LocalPose != newState.LocalPose) {
                iEntity.LocalPose().ApplyTo(go.transform);
            }
            if (oldState.IsActive != newState.IsActive) {
                go.SetActive(newState.IsActive);
            }
            oldState.Components.CalcEntryChangesV2(newState.Components,
                added => onCompAdded(iEntity, added, targetParentGo: go),
                updated => onCompUpdated(iEntity, oldState.Components[updated.Key], updated, targetParentGo: go),
                deleted => onCompRemoved(iEntity, deleted, targetParentGo: go)
            );
        }

        private void onCompAdded(IEntity<T> iEntity, KeyValuePair<string, IComponentData> added, GameObject targetParentGo) {
            var createdComponent = AddComponentTo(targetParentGo, added.Value);
            _componentViews.Add(added.Key, createdComponent);
            createdComponent.OnUpdateUnityComponent(default, added.Value);
        }

        protected abstract IComponentPresenter AddComponentTo(GameObject targetGo, IComponentData componentModel);

        private void onCompRemoved(IEntity<T> iEntity, string deleted, GameObject targetParentGo) {
            _componentViews[deleted].DisposeV2();
            _componentViews.Remove(deleted);
        }

        private void onCompUpdated(IEntity<T> iEntity, IComponentData oldState, KeyValuePair<string, IComponentData> updatedState, GameObject targetParentGo) {
            var compView = _componentViews[updatedState.Key];
            compView.OnUpdateUnityComponent(oldState, updatedState.Value);
        }

    }

    public interface IComponentPresenter : IDisposableV2 {
        void OnUpdateUnityComponent(IComponentData? oldState, IComponentData updatedState);
    }

    public static class PoseExtensionsForUnity {

        public static void ApplyTo(this Pose self, Transform goTransform) {
            goTransform.SetLocalPositionAndRotation(self.position.ToUnityVec(), self.rotation.ToUnityRot());
            goTransform.localScale = self.scale.ToUnityVec();
        }

        public static Quaternion ToUnityRot(this System.Numerics.Quaternion self) {
            return new Quaternion(self.X, self.Y, self.Z, self.W);
        }

        public static Vector3 ToUnityVec(this System.Numerics.Vector3 self) {
            return new Vector3(self.X, self.Y, self.Z);
        }

    }

}