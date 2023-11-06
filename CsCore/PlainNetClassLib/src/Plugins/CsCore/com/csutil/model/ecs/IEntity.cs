using System.Collections.Generic;
using System.Numerics;

namespace com.csutil.model.ecs {

    public interface IEntity<T> : IEntityData, IDisposableV2 where T : IEntityData {

        T Data { get; }
        EntityComponentSystem<T> Ecs { get; }

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

    public struct Pose {

        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 scale;

        public Pose(Vector3 position, Quaternion rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public static Matrix4x4 NewMatrix(Vector3 position = new Vector3(), float rotation = 0, float scale = 1f) {
            return NewMatrix(position, Quaternion.CreateFromYawPitchRoll(rotation, 0, 0), scale);
        }

        public static Matrix4x4 NewMatrix(Vector3 position, Quaternion rotation, float scale) {
            return NewMatrix(position, rotation, new Vector3(scale, scale, scale));
        }

        public static Matrix4x4 NewMatrix(Vector3 position, Quaternion rotation, Vector3 scale) {
            return Matrix4x4Extensions.Compose(position, rotation, scale);
        }

        public static Pose NewPosition(Vector3 position) {
            return new Pose(position, Quaternion.Identity, Vector3.One);
        }

    }

}