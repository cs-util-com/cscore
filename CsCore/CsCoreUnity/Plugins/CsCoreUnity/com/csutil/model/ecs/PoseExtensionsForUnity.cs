using UnityEngine;

namespace com.csutil.model.ecs {
    
    public static class PoseExtensionsForUnity {

        public static void ApplyTo(this Pose3d self, Transform goTransform) {
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