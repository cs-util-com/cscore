using System.Collections.Generic;
using System.Numerics;

namespace com.csutil.model.ecs {

    public interface IEntity<T> : IEntityData where T : IEntityData {

        T Data { get; }
        EntityComponentSystem<T> Ecs { get; }

    }

    public interface IEntityData : HasId {

        string Id { get; }
        string TemplateId { get; }
        Matrix4x4? LocalPose { get; }
        IReadOnlyDictionary<string, IComponentData> Components { get; }
        string ParentId { get; }
        IReadOnlyList<string> ChildrenIds { get; }
        string Name { get; }

    }

    public interface IComponentData : HasId {

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