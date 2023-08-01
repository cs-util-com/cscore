using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace com.csutil.model.ecs {

    public static class IEntityExtensions {

        public static IEnumerable<IEntity<T>> GetChildren<T>(this IEntity<T> self) where T : IEntityData {
            if (self.ChildrenIds == null) { return null; }
            return self.ChildrenIds.Map(x => self.Ecs.GetEntity(x));
        }

        public static IEntity<T> GetParent<T>(this IEntity<T> self) where T : IEntityData {
            if (!self.Ecs.AllParentIds.ContainsKey(self.Id)) { return null; }
            return self.Ecs.GetParentOf(self.Id);
        }

        public static IEntity<T> AddChild<T>(this IEntity<T> parent, T childData, Func<IEntity<T>, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            var newChild = parent.Ecs.Add(childData);
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent, childData.Id));
            return newChild;
        }

        public static IEntity<T> AddChild<T>(this IEntity<T> parent, IEntity<T> existingChild, Func<IEntity<T>, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            if (parent.Ecs.AllParentIds.ContainsKey(existingChild.Id)) {
                throw new InvalidOperationException("The child is already connected to a parent");
            }
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent, existingChild.Id));
            return existingChild;
        }

        // SetParent:
        public static IEntity<T> SetParent<T>(this IEntity<T> child, IEntity<T> newParent, Func<IEntity<T>, string, T> removeChildIdFromOldParent, Func<IEntity<T>, string, T> mutateChildrenListInNewParent) where T : IEntityData {
            if (child.Ecs.AllParentIds.ContainsKey(child.Id)) {
                child.RemoveFromParent(removeChildIdFromOldParent);
            }
            return newParent.AddChild(child, mutateChildrenListInNewParent);
        }

        public static bool Destroy<T>(this IEntity<T> self, Func<IEntity<T>, string, T> removeChildIdFromParent) where T : IEntityData {
            if (self.IsDestroyed()) { return false; }
            self.RemoveFromParent(removeChildIdFromParent);
            self.DestroyAllChildrenRecursively(removeChildIdFromParent);
            self.Ecs.Destroy(self.Id);
            return true;
        }

        private static void DestroyAllChildrenRecursively<T>(this IEntity<T> self, Func<IEntity<T>, string, T> removeChildIdFromParent) where T : IEntityData {
            var children = self.GetChildren().ToList();
            if (children != null) {
                foreach (var child in children) {
                    child.Destroy(removeChildIdFromParent);
                }
            }
        }

        public static bool IsDestroyed<T>(this IEntity<T> self) where T : IEntityData {
            return self.Ecs == null;
        }

        public static void RemoveFromParent<T>(this IEntity<T> child, Func<IEntity<T>, string, T> removeFromParent) where T : IEntityData {
            var parent = child.GetParent();
            if (parent != null) {
                var updatedParent = removeFromParent(parent, child.Id);
                child.Ecs.Update(updatedParent);
            }
            // The parent cant tell the ecs anymore that the ParentIds list needs to be updated so the child needs to do this: 
            child.Ecs.Update(child.Data);
        }

        /// <summary> Combines the local pose of the entity with the pose of all its parents </summary>
        public static Matrix4x4 GlobalPoseMatrix<T>(this IEntity<T> self) where T : IEntityData {
            var lp = self.LocalPose;
            Matrix4x4 localPose = lp.HasValue ? lp.Value : Matrix4x4.Identity;
            var parent = self.GetParent();
            if (parent == null) { return localPose; }
            return localPose * parent.GlobalPoseMatrix();
        }

        public static Pose GlobalPose<T>(this IEntity<T> self) where T : IEntityData {
            self.GlobalPoseMatrix().Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
            return new Pose(position, rotation, scale);
        }

        public static Pose LocalPose<T>(this IEntity<T> self) where T : IEntityData {
            var localPose = self.LocalPose;
            if (!localPose.HasValue) { return new Pose(Vector3.Zero, Quaternion.Identity, Vector3.One); }
            localPose.Value.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
            return new Pose(position, rotation, scale);
        }

        public static void SaveChanges<T>(this IEntity<T> self) where T : IEntityData {
            var fullSubtree = self.GetChildrenTreeBreadthFirst();
            foreach (var e in fullSubtree) { e.Ecs.SaveChanges(e.Data); }
        }

        public static IEntity<T> CreateVariant<T>(this IEntity<T> self) where T : IEntityData {
            return self.Ecs.CreateVariant(self.Data);
        }

        public static IEntity<T> GetChild<T>(this IEntity<T> mageEnemy, string name) where T : IEntityData {
            return mageEnemy.GetChildren().Single(x => x.Name == name);
        }

        /// <summary> Returns the full subtree under the entity in a depth first order </summary>
        public static IEnumerable<IEntity<T>> GetChildrenTreeBreadthFirst<T>(this IEntity<T> self) where T : IEntityData {
            return TreeFlattenTraverse.BreadthFirst(self, x => x.GetChildren());
        }

        /// <summary> Recursively searches for all components of the specified type in the entity and all its children </summary>
        public static IEnumerable<V> GetComponentsInChildren<T, V>(this IEntity<T> self) where T : IEntityData where V : IComponentData {
            return self.GetChildrenTreeBreadthFirst().SelectMany(x => x.Components.Values).Where(c => c is V).Cast<V>();
        }

        /// <summary> Recursively searches the entity and all its children until a component of the specified type is found </summary>
        public static V GetComponentInChildren<T, V>(this IEntity<T> self) where T : IEntityData where V : IComponentData {
            return self.GetComponentsInChildren<T, V>().FirstOrDefault();
        }

    }

    public static class EcsExtensions {

        public static IEnumerable<IEntity<T>> FindEntitiesWithName<T>(this EntityComponentSystem<T> ecs, string name) where T : IEntityData {
            return ecs.AllEntities.Values.Filter(x => x.Name == name);
        }

    }

    public static class IEntityDataExtensions {

        public static V GetComponent<V>(this IEntityData self) where V : class {
            return self.Components.Values.Single(c => c is V) as V;
        }

    }

}