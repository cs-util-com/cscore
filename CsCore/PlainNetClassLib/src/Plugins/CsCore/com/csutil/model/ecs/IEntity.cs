using System;
using System.Collections.Generic;
using System.Numerics;

namespace com.csutil.model.ecs {

    public interface IEntity<T> : IEntityData, IDisposableV2 where T : IEntityData {

        T Data { get; }
        EntityComponentSystem<T> Ecs { get; }
        
        /// <summary> Optional callback listener that informs the IEntity if its
        /// content (the entity data) was updated. Will not fire for the initial creation of
        /// the entity or the removal/destruction of the entity in the ecs </summary>
        Action<T,T> OnUpdate { get; set; }

    }

    public interface IEntityData : HasId {

        /// <summary> The unique id of the entity </summary>
        string Id { get; }

        /// <summary> The name of the entity </summary>
        string Name { get; }

        /// <summary> If the entity is a variant of a template,
        /// this is the id of the template </summary>
        string TemplateId { get; }

        /// <summary> The local position in the parent entity.
        /// If the entity has no parent, this is equal to the global position. </summary>
        Matrix4x4? LocalPose { get; }

        /// <summary> The collection of all components of the entity </summary>
        IReadOnlyDictionary<string, IComponentData> Components { get; }

        /// <summary> If the entity is a child of another entity,
        /// this is the id of the parent entity </summary>
        string ParentId { get; }

        /// <summary> The ids of all child entities of this entity </summary>
        IReadOnlyList<string> ChildrenIds { get; }

        /// <summary> This flag indicates if the entity is considered active or not.
        /// An inactive entity typically is frozen and not visible </summary>
        bool IsActive { get; }

    }

    public interface IComponentData : HasId {

        /// <summary> This flag indicates if the component is considered active or not.
        /// An inactive component typically is frozen.
        /// Both an entity and individual components can be inactive. </summary>
        bool IsActive { get; }

    }

    public struct Pose3d {

        public const double radToDegree = 180f / Math.PI;
        public const double degreeToRad = Math.PI / 180f;

        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 scale;

        public Pose3d(Vector3 position, Quaternion rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public static Matrix4x4 NewMatrix(Vector3 position = new Vector3(), double rotOnYAxisInDegree = 0, float scale = 1f) {
            var rot = rotOnYAxisInDegree == 0 ? Quaternion.Identity : Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(rotOnYAxisInDegree * degreeToRad));
            return NewMatrix(position, rot, scale);
        }

        public static Matrix4x4 NewMatrix(Vector3 position, Quaternion rotation, float scale) {
            return NewMatrix(position, rotation, new Vector3(scale, scale, scale));
        }

        public static Matrix4x4 NewMatrix(Vector3 position, Quaternion rotation, Vector3 scale) {
            return Matrix4x4Extensions.Compose(position, rotation, scale);
        }

        public static Pose3d NewPosition(Vector3 position) {
            return new Pose3d(position, Quaternion.Identity, Vector3.One);
        }

        public static Pose3d operator +(Pose3d pose, Vector3 positionToAdd) {
            return new Pose3d(pose.position + positionToAdd, pose.rotation, pose.scale);
        }

        public static Pose3d operator *(Pose3d pose, Vector3 scaleToMultiply) {
            return new Pose3d(pose.position, pose.rotation, pose.scale * scaleToMultiply);
        }

        public static Pose3d operator *(Pose3d pose, float scaleToMultiply) {
            return new Pose3d(pose.position, pose.rotation, pose.scale * scaleToMultiply);
        }

        public static Pose3d operator *(Quaternion rotationToAdd, Pose3d pose) {
            return new Pose3d(pose.position, rotationToAdd * pose.rotation, pose.scale);
        }

        public Matrix4x4 ToMatrix4x4() { return NewMatrix(position, rotation, scale); }

    }

}