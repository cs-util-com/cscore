﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace com.csutil.model.ecs {

    public static class IEntityExtensions {

        public static IEnumerable<IEntity<T>> GetChildren<T>(this IEntity<T> self) where T : IEntityData {
            if (self.ChildrenIds == null) { return null; }
            return self.ChildrenIds.Map(x => self.Ecs.GetEntity(x));
        }

        public static IEntity<T> GetParent<T>(this IEntity<T> self) where T : IEntityData {
            if (self.ParentId == null) { return default; }
            return self.Ecs.Entities[self.ParentId];
        }

        public static T GetParent<T>(this T self, IReadOnlyDictionary<string, T> allEntities) where T : IEntityData {
            if (self.ParentId == null) { return default; }
            return allEntities[self.ParentId];
        }

        public static IEntity<T> AddChild<T>(this IEntity<T> parent, T childData, Func<IEntity<T>, string, T> setParentIdInChild, Func<IEntity<T>, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            var newChild = parent.Ecs.Add(childData);
            if (newChild.ParentId != parent.Id) {
                parent.Ecs.Update(setParentIdInChild(newChild, parent.Id));
            }
            if (newChild.ParentId != parent.Id) {
                throw new ArgumentException("The childData.ParentId must be the same as the parent.Id: " + parent.Id + " != " + newChild.ParentId);
            }
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent, childData.Id));
            return newChild;
        }

        public static IEntity<T> AddChild<T>(this IEntity<T> parent, IEntity<T> existingChild, Func<IEntity<T>, string, T> setParentIdInChild, Func<IEntity<T>, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            if (existingChild.ParentId != null) {
                throw new InvalidOperationException("Parent already set to " + existingChild.ParentId);
            }
            if (existingChild.ParentId != parent.Id) {
                parent.Ecs.Update(setParentIdInChild(existingChild, parent.Id));
            }
            if (existingChild.ParentId != parent.Id) {
                throw new ArgumentException("The childData.ParentId must be the same as the parent.Id: " + parent.Id + " != " + existingChild.ParentId);
            }
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent, existingChild.Id));
            return existingChild;
        }

        public static IEntity<T> SetParent<T>(this IEntity<T> child, IEntity<T> newParent,
            Func<IEntity<T>, string, T> setParentIdInChild,
            Func<IEntity<T>, string, T> removeChildIdFromOldParent,
            Func<IEntity<T>, string, T> mutateChildrenListInNewParent) where T : IEntityData {
            {
                if (child.ParentId == newParent.Id) { return child; }
                // Remove the child from the old parent:
                if (child.ParentId != null) {
                    child.RemoveFromParent(c => c.Data, removeChildIdFromOldParent);
                }
                child.Ecs.Update(setParentIdInChild(child, newParent.Id));
                newParent.Ecs.Update(mutateChildrenListInNewParent(newParent, child.Id));
                return child;
            }
        }

        public static bool Destroy<T>(this IEntity<T> self, Func<IEntity<T>, string, T> removeChildIdFromParent) where T : IEntityData {
            if (self.IsDestroyed()) { return false; }
            if (self.ParentId != null) {
                self.RemoveFromParent(c => c.Data, removeChildIdFromParent);
            }
            self.DestroyAllChildrenRecursively(removeChildIdFromParent);
            self.Ecs.Destroy(self);
            return true;
        }

        private static void DestroyAllChildrenRecursively<T>(this IEntity<T> self, Func<IEntity<T>, string, T> removeChildIdFromParent) where T : IEntityData {
            var children = self.GetChildren();
            if (children != null) {
                var childrenToDelete = children.ToList();
                foreach (var child in childrenToDelete) {
                    child.Destroy(removeChildIdFromParent);
                }
            }
        }

        public static bool IsDestroyed<T>(this IEntity<T> self) where T : IEntityData {
            return self.Ecs == null;
        }

        public static void RemoveFromParent<T>(this IEntity<T> child, Func<IEntity<T>, T> removeParentIdFromChild, Func<IEntity<T>, string, T> removeChildIdFromParent) where T : IEntityData {
            var parent = child.GetParent();
            if (parent == null) {
                throw new ArgumentException("The child " + child.Id + " has no parent");
            }
            var updatedParent = removeChildIdFromParent(parent, child.Id);
            child.Ecs.Update(updatedParent);
            // The parent cant tell the ecs anymore that the ParentIds list needs to be updated so the child needs to do this: 
            var updatedChild = removeParentIdFromChild(child);
            child.Ecs.Update(updatedChild);
        }

        /// <summary> Combines the local pose of the entity with the pose of all its parents </summary>
        public static Matrix4x4 GlobalPoseMatrix<T>(this IEntity<T> self) where T : IEntityData {
            var lp = self.LocalPose;
            Matrix4x4 localPose = lp.HasValue ? lp.Value : Matrix4x4.Identity;
            var parent = self.GetParent();
            if (parent == null) { return localPose; }
            return localPose * parent.GlobalPoseMatrix();
        }

        /// <summary> Combines the local pose of the entity with the pose of all its parents </summary>
        public static Matrix4x4 GlobalPoseMatrix<T>(this T self, IReadOnlyDictionary<string, T> allEntities) where T : IEntityData {
            var lp = self.LocalPose;
            Matrix4x4 localPose = lp.HasValue ? lp.Value : Matrix4x4.Identity;
            var parent = self.GetParent(allEntities);
            if (parent == null) { return localPose; }
            return localPose * parent.GlobalPoseMatrix(allEntities);
        }

        public static Pose GlobalPose<T>(this IEntity<T> self) where T : IEntityData {
            self.GlobalPoseMatrix().Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
            return new Pose(position, rotation, scale);
        }

        public static Pose GlobalPose<T>(this T self, IReadOnlyDictionary<string, T> allEntities) where T : IEntityData {
            self.GlobalPoseMatrix(allEntities).Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
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
            foreach (var e in fullSubtree) { e.Ecs.Update(e.Data); }
        }

        /// <summary>
        /// Recursively creates variants of all entities in the subtree of the entity and returns a new root entity that has the variant ids in its children lists
        /// </summary>
        public static IEntity<T> CreateVariant<T>(this IEntity<T> self) where T : IEntityData {
            var all = self.GetChildrenTreeBreadthFirst().ToList();
            var newIdsLookup = all.ToDictionary(x => x.Id, x => "" + GuidV2.NewGuid());
            var fullSubtreeLeavesFirst = all.Skip(1).Reverse();
            foreach (var e in fullSubtreeLeavesFirst) {
                e.Ecs.CreateVariant(e.Data, newIdsLookup);
            }
            return self.Ecs.CreateVariant(self.Data, newIdsLookup);
        }

        public static IEntity<T> GetChild<T>(this IEntity<T> mageEnemy, string name) where T : IEntityData {
            return mageEnemy.GetChildren().Single(x => x.Name == name);
        }

        /// <summary> Returns the full subtree under the entity in a breath first order </summary>
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
            return ecs.Entities.Values.Filter(x => x.Name == name);
        }

    }

    public static class IEntityDataExtensions {

        public static V GetComponent<V>(this IEntityData self) where V : IComponentData {
            if (self is IDisposableV2 d && !d.IsAlive()) {
                throw new InvalidOperationException($"The entity {self.Id} is already disposed");
            }
            AssertOnlySingleCompOfType<V>(self);
            // Take a shortcut for the common case where the most requested component has the same id as the entity:
            var comps = self.Components;
            if (comps.TryGetValue(self.Id, out var c) && c is V v) { return v; }
            // Else go through the list of all components:
            return (V)comps.Values.SingleOrDefault(comp => comp is V);
        }

        public static bool TryGetComponent<V>(this IEntityData self, out V comp) where V : IComponentData {
            var comps = self.Components;
            // Take a shortcut for the common case where the most requested component has the same id as the entity:
            if (comps.TryGetValue(self.Id, out var comp2) && comp2 is V v) {
                comp = v;
                return true;
            }
            var compOrNull = comps.Values.SingleOrDefault(c => c is V);
            if (compOrNull != null) {
                comp = (V)compOrNull;
                return true;
            }
            comp = default;
            return false;
        }

        [Conditional("DEBUG")]
        private static void AssertOnlySingleCompOfType<V>(IEntityData self) where V : IComponentData {
            self.ThrowErrorIfNull("Entity self");
            var compTypeCount = self.Components.Values.Count(c => c is V);
            if (compTypeCount > 1) {
                throw new ArgumentException($"The entity {self.Id} has {compTypeCount} components of type {typeof(V).Name} but only one is allowed");
            }
        }

    }

}