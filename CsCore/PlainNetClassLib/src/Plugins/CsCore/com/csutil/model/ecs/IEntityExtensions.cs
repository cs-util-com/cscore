using System;
using System.Collections.Generic;
using System.Numerics;

namespace com.csutil.model.ecs {
    
    public static class IEntityExtensions {

        public static IEnumerable<IEntity<T>> GetChildren<T>(this IEntity<T> self) where T : IEntityData {
            return self.ChildrenIds.Map(x => self.Ecs.GetEntity(x));
        }

        public static IEntity<T> GetParent<T>(this IEntity<T> self) where T : IEntityData {
            if (!self.Ecs.AllParentIds.ContainsKey(self.Id)) { return null; }
            return self.Ecs.GetParentOf(self.Id);
        }

        public static IEntity<T> AddChild<T>(this IEntity<T> parent, T childData, Func<T, string, T> mutateChildrenListInParentEntity) where T : IEntityData {
            var newChild = parent.Ecs.Add(childData);
            parent.Ecs.Update(mutateChildrenListInParentEntity(parent.Data, childData.Id));
            return newChild;
        }

        public static bool Destroy<T>(this IEntity<T> self, Func<T, string, T> removeChildIdFromParent) where T : IEntityData {
            if (self.IsDestroyed()) { return false; }
            self.RemoveFromParent(removeChildIdFromParent);
            self.Ecs.Destroy(self.Id);
            return true;
        }

        public static bool IsDestroyed<T>(this IEntity<T> self) where T : IEntityData {
            return self.Ecs == null;
        }

        public static void RemoveFromParent<T>(this IEntity<T> child, Func<T, string, T> removeFromParent) where T : IEntityData {
            var parent = child.GetParent();
            if (parent != null) {
                var updatedParent = removeFromParent(parent.Data, child.Id);
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

    }
    
}