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
        IReadOnlyList<IComponentData> Components { get; }
        IReadOnlyList<string> ChildrenIds { get; }
        IReadOnlyList<string> Tags { get; }

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

    }

}