using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace com.csutil.model.ecs {

    public static class IEntityExtensions {

        public static IEnumerable<IEntity<T>> GetChildren<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            if (self.ChildrenIds == null) { return null; }
            return self.ChildrenIds.Map(x => self.Ecs.GetEntity(x));
        }

        public static IEntity<T> GetParent<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            if (self.ParentId == null) { return default; }
            return self.Ecs.Entities[self.ParentId];
        }

        public static bool IsTemplate<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.Ecs.TemplateIds.Contains(self.Id);
        }

        public static bool IsVariant<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.TemplateId != null;
        }

        public static bool TryGetTemplate<T>(this IEntity<T> self, out IEntity<T> template) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            if (self.TemplateId != null) {
                template = self.Ecs.Entities[self.TemplateId];
                return true;
            }
            template = default;
            return false;
        }

        public static bool TryGetVariants<T>(this IEntity<T> self, out IEnumerable<IEntity<T>> variants) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.Ecs.TryGetVariants(self.Id, out variants);
        }

        public static T GetParent<T>(this T self, IReadOnlyDictionary<string, T> allEntities) where T : IEntityData {
            if (self.ParentId == null) { return default; }
            return allEntities[self.ParentId];
        }

        /// <summary> This method is typically used for newly created entities to add them to the ecs. </summary>
        public static IEntity<T> AddChild<T>(this IEntity<T> parent, T newEntityData, Func<IEntity<T>, string, T> setParentIdInChild, Func<IEntity<T>, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            parent.ThrowErrorIfDisposed();
            if (newEntityData.ParentId != null) { // The new entity should not have a parent yet
                throw new InvalidOperationException("Parent already set to " + newEntityData.ParentId);
            }
            var newChild = parent.Ecs.Add(newEntityData);
            if (newChild.ParentId != parent.Id) {
                parent.Ecs.Update(setParentIdInChild(newChild, parent.Id));
            }
            if (newChild.ParentId != parent.Id) {
                throw new ArgumentException("The childData.ParentId must be the same as the parent.Id: " + parent.Id + " != " + newChild.ParentId);
            }
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent, newEntityData.Id));
            return newChild;
        }

        /// <summary> This method is typically used to add an existing entity to a parent entity. </summary>
        /// <returns> The updated child entity </returns>
        public static IEntity<T> AddChild<T>(this IEntity<T> parent, IEntity<T> existingChild, Func<IEntity<T>, string, T> setParentIdInChild, Func<IEntity<T>, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            parent.ThrowErrorIfDisposed();
            existingChild.ThrowErrorIfDisposed();
            if (existingChild.ParentId != null) {
                throw new InvalidOperationException("Parent already set to " + existingChild.ParentId);
            }
            if (parent.Id == existingChild.Id) {
                throw new InvalidOperationException("The parent and the child are the same entity");
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
                child.ThrowErrorIfDisposed();
                newParent.ThrowErrorIfNull("newParent");
                newParent.ThrowErrorIfDisposed();
                if (newParent.Id == child.Id) {
                    throw new InvalidOperationException("The new parent and the child are the same entity");
                }
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

        public static void SetActiveSelf<T>(this IEntity<T> self, bool newIsActive, Func<IEntity<T>, bool, T> setActive) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            if (self.IsActive == newIsActive) { return; }
            self.Ecs.Update(setActive(self, newIsActive));
        }

        public static async Task<bool> Destroy<T>(this IEntity<T> self, Func<IEntity<T>, string, T> removeChildIdFromParent, Func<IEntity<T>, string, T> changeTemplate) where T : IEntityData {
            if (self.IsDestroyed()) { return false; }

            var ecs = self.Ecs;
            var newTemplate = self.TemplateId;
            if (ecs.TryGetVariants(self.Id, out var variants)) {
                foreach (var variant in variants) {
                    var updatedVariant = changeTemplate(variant, newTemplate);
                    await ecs.Update(updatedVariant);
                }
            }

            if (self.ParentId != null) {
                self.RemoveFromParent(c => c.Data, removeChildIdFromParent);
            }
            await self.DestroyAllChildrenRecursively(removeChildIdFromParent, changeTemplate);
            await ecs.Destroy(self);
            return true;
        }

        private static async Task DestroyAllChildrenRecursively<T>(this IEntity<T> self, Func<IEntity<T>, string, T> removeChildIdFromParent, Func<IEntity<T>, string, T> changeTemplate) where T : IEntityData {
            var children = self.GetChildren();
            if (children != null) {
                var childrenToDelete = children.ToList();
                foreach (var child in childrenToDelete) {
                    await child.Destroy(removeChildIdFromParent, changeTemplate);
                }
            }
        }

        public static bool IsDestroyed<T>(this IEntity<T> self) where T : IEntityData {
            var result = !self.IsAlive();
            if (result != (self.Ecs == null)) {
                throw Log.e("IsAlive()=" + result + " but Ecs=" + self.Ecs);
            }
            return result;
        }

        public static void RemoveFromParent<T>(this IEntity<T> child, Func<IEntity<T>, T> removeParentIdFromChild, Func<IEntity<T>, string, T> removeChildIdFromParent) where T : IEntityData {
            child.ThrowErrorIfDisposed();
            var parent = child.GetParent();
            if (parent == null) {
                throw new ArgumentException("The child " + child.Id + " has no parent");
            }
            AssertV3.IsTrue(parent.ChildrenIds.Contains(child.Id), () => "The parent " + parent.Id + " does not contain the child " + child.Id);
            var updatedParent = removeChildIdFromParent(parent, child.Id);
            AssertV3.IsFalse(parent.ChildrenIds.Contains(child.Id), () => "The parent " + parent.Id + " still contains the child " + child.Id);

            child.Ecs.Update(updatedParent);
            // The parent cant tell the ecs anymore that the ParentIds list needs to be updated so the child needs to do this: 
            var updatedChild = removeParentIdFromChild(child);
            child.Ecs.Update(updatedChild);
        }

        /// <summary> Combines the local pose of the entity with the pose of all its parents </summary>
        public static Matrix4x4 GlobalPoseMatrix<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
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

        public static Matrix4x4 CalcLocalPoseInParent<T>(this IEntity<T> self, Matrix4x4 globalPose) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            var parent = self.GetParent();
            if (parent == null) { return globalPose; }
            return parent.ToLocalPose(globalPose);
        }
        
        public static Matrix4x4 ToLocalPose<T>(this IEntity<T> self, Matrix4x4 globalPose) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.GlobalPoseMatrix().Inverse() * globalPose;
        }
        
        // ToGlobalPose (which takes an IEntity in and a local pose in that entities space and calculates the global pose)
        public static Matrix4x4 ToGlobalPose<T>(this IEntity<T> self, Matrix4x4 localPose) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.GlobalPoseMatrix() * localPose;
        }

        public static Pose3d GlobalPose<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.GlobalPoseMatrix().ToPose();
        }

        public static Pose3d ToPose(this Matrix4x4? matrix) {
            if (matrix == null) { return null; }
            return matrix.Value.ToPose();
        }

        public static Pose3d ToPose(this Matrix4x4 matrix) {
            if (matrix == Matrix4x4.Identity) {
                return new Pose3d(Vector3.Zero, Quaternion.Identity, Vector3.One);
            }
            matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);
            return new Pose3d(position, rotation, scale);
        }

        public static Pose3d GlobalPose<T>(this T self, IReadOnlyDictionary<string, T> allEntities) where T : IEntityData {
            return self.GlobalPoseMatrix(allEntities).ToPose();
        }

        public static Pose3d LocalPose<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            var localPose = self.LocalPose;
            if (!localPose.HasValue) { return Matrix4x4.Identity.ToPose(); }
            return localPose.Value.ToPose();
        }

        public static Task SaveChanges<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            var fullSubtree = self.GetSelfAndChildrenTreeBreadthFirst();
            var tasks = new List<Task>();
            foreach (var e in fullSubtree) { tasks.Add(e.Ecs.Update(e.Data)); }
            return Task.WhenAll(tasks);
        }

        public static IEntity<T> CreateVariant<T>(this IEntity<T> self) where T : IEntityData {
            return self.CreateVariant(out _);
        }

        /// <summary>
        /// Recursively creates variants of all entities in the subtree of the entity and returns a new root entity that has the variant ids in its children lists
        /// </summary>
        public static IEntity<T> CreateVariant<T>(this IEntity<T> self, out IReadOnlyDictionary<string, string> resultIdLookupTable) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            var all = self.GetSelfAndChildrenTreeBreadthFirst().ToList();
            resultIdLookupTable = all.ToDictionary(x => x.Id, x => "" + GuidV2.NewGuid());
            var fullSubtreeLeavesFirst = all.Skip(1).Reverse();
            foreach (var e in fullSubtreeLeavesFirst) {
                e.Ecs.CreateVariant(e.Data, resultIdLookupTable);
            }
            var result = self.Ecs.CreateVariant(self.Data, resultIdLookupTable);
            AssertV3.IsNull(result.ParentId, "result.ParentId");
            AssertV3.AreNotEqual(result.Id, self.Id);
            return result;
        }

        public static IEntity<T> GetChild<T>(this IEntity<T> self, string name) where T : IEntityData {
            return self.GetChildren().Single(x => x.Name == name);
        }

        /// <summary> Returns the full subtree under the entity in a breath first order </summary>
        public static IEnumerable<IEntity<T>> GetSelfAndChildrenTreeBreadthFirst<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return TreeFlattenTraverse.BreadthFirst(self, x => x.GetChildren());
        }

        /// <summary> Recursively searches for all components of the specified type in the entity and all its children </summary>
        public static IEnumerable<V> GetComponentsInSelfAndChildren<T, V>(this IEntity<T> self) where T : IEntityData {
            return self.GetSelfAndChildrenTreeBreadthFirst().SelectMany(x => x.Components.Values).Where(c => c is V).Cast<V>();
        }

        /// <summary> Recursively searches the entity and all its children until a component of the specified type is found </summary>
        public static V GetComponentInSelfAndChildren<T, V>(this IEntity<T> self) where T : IEntityData {
            return self.GetComponentsInSelfAndChildren<T, V>().FirstOrDefault();
        }

        public static bool TryGetComponentInParents<T, V>(this IEntity<T> self, out V foundComp, out IEntity<T> parent) where T : IEntityData where V : IComponentData {
            foreach (var parentId in self.CollectAllParents()) {
                parent = self.Ecs.GetEntity(parentId);
                if (parent.TryGetComponent(out foundComp)) {
                    return true;
                }
            }
            parent = default;
            foundComp = default;
            return false;
        }

        public static bool IsActiveSelf<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.IsActive;
        }

        public static bool IsActiveSelf(this IEntityData self) {
            return self.IsActive;
        }

        public static bool IsActiveInHierarchy<T>(this IEntity<T> self) where T : IEntityData {
            self.ThrowErrorIfDisposed();
            return self.IsActive && self.CollectAllParents().Map(id => self.Ecs.GetEntity(id)).All(x => x.IsActive);
        }

    }

    public static class EcsExtensions {

        public static IEnumerable<IEntity<T>> FindEntitiesWithName<T>(this EntityComponentSystem<T> ecs, string name) where T : IEntityData {
            return ecs.Entities.Values.Filter(x => x.Name == name);
        }

        public static bool TryFindCommonParent<T>(this EntityComponentSystem<T> ecs, IEnumerable<string> entityIds, out IEntity<T> commonParent) where T : IEntityData {
            // While walking up for each entity in the list, collec all its parents in a lookup table:
            var entities = entityIds.Select(x => ecs.Entities[x]);
            var intersectingIds = entities.First().CollectAllParents();
            foreach (var entity in entities.Skip(1)) {
                intersectingIds.Intersect(entity.CollectAllParents());
            }
            if (intersectingIds.Count == 0) {
                commonParent = default;
                return false;
            }
            commonParent = ecs.Entities[intersectingIds.First()];
            return true;
        }

        public static IReadOnlyList<string> CollectAllParents<T>(this IEntity<T> entity) where T : IEntityData {
            return CollectAllParents(entity, new List<string>());
        }

        private static IReadOnlyList<string> CollectAllParents<T>(IEntity<T> entity, List<string> parentsLookup) where T : IEntityData {
            if (entity.ParentId != null) {
                parentsLookup.Add(entity.ParentId);
                CollectAllParents(entity.GetParent(), parentsLookup);
            }
            return parentsLookup;
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

        public static bool TryGetComponent<V>(this IEntityData self, out V comp) {
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

        public static bool TryGetComponents<V>(this IEntityData self, out IEnumerable<V> components) {
            components = self.Components.Values.OfType<V>();
            return components.Any();
        }

        public static bool HasComponent<V>(this IEntityData self) where V : IComponentData {
            return self.Components.Values.Any(c => c is V);
        }

        [Conditional("DEBUG")]
        private static void AssertOnlySingleCompOfType<V>(IEntityData self) where V : IComponentData {
            self.ThrowErrorIfNull("Entity self");
            var compTypeCount = self.Components.Values.Count(c => c is V);
            if (compTypeCount > 1) {
                throw new ArgumentException($"The entity {self.Id} has {compTypeCount} components of type {typeof(V).Name} but only one is allowed");
            }
        }

        public static bool IsNullOrInactive(this IComponentData self) {
            return self == null || !self.IsActive;
        }

        public static bool IsActiveSelf(this IComponentData self) {
            return self.IsActive;
        }

    }

}