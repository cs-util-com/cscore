using System.Diagnostics;
using UnityEngine;

namespace com.csutil.model.ecs {

    public static class PoseExtensionsForUnity {

        public static void ApplyTo(this Pose3d pose3d, Transform goTransform) {
            AssertV3.IsNotNull(pose3d, "pose3d");
            var newLocalPosition = pose3d.position.ToUnityVec();
            var newLocalRotation = pose3d.rotation.ToUnityRot();
            var newLocalScale = pose3d.scale.ToUnityVec();
            var positionChanged = goTransform.localPosition != newLocalPosition;
            var rotationChanged = goTransform.localRotation == newLocalRotation;
            var scaleChanged = goTransform.localScale != newLocalScale;
            // No need to update the transform if the values are already the same:
            if (!positionChanged && !rotationChanged && !scaleChanged) { return; }
            if (scaleChanged) {
                goTransform.localScale = newLocalScale;
            }
            if (positionChanged && rotationChanged) {
                goTransform.SetLocalPositionAndRotation(newLocalPosition, newLocalRotation);
            } else if (positionChanged) {
                goTransform.localPosition = newLocalPosition;
            } else if (rotationChanged) {
                goTransform.localRotation = newLocalRotation;
            }
            AssertAfterPoseUpdatePresenterAndModelInSync(pose3d, goTransform);
        }

        [Conditional("DEBUG")]
        private static void AssertAfterPoseUpdatePresenterAndModelInSync(Pose3d newLocalPose3d, Transform goTransform) {
            var goTransformPose3d = goTransform.ToLocalPose3d();
            if ((goTransformPose3d.position - newLocalPose3d.position).Length() > 0.01f) {
                Log.e($"After Unity presenter update local Unity pos is not same as entity local pos: "
                    + $"\n newPose3d={newLocalPose3d} "
                    + $"\n transform={goTransformPose3d}", goTransform.gameObject);
            }
        }

        public static Pose3d ToLocalPose3d(this Transform self) {
            return new Pose3d(ToNumericsVec(self.localPosition), ToNumericsRot(self.localRotation), ToNumericsVec(self.localScale));
        }

        public static Pose3d ToGlobalPose3d(this Transform self) {
            return new Pose3d(ToNumericsVec(self.position), ToNumericsRot(self.rotation), ToNumericsVec(self.lossyScale));
        }

        public static System.Numerics.Vector3 ToNumericsVec(this Vector3 self) {
            return new System.Numerics.Vector3(self.x, self.y, self.z);
        }

        public static System.Numerics.Quaternion ToNumericsRot(this Quaternion self) {
            return new System.Numerics.Quaternion(self.x, self.y, self.z, self.w);
        }

        public static Quaternion ToUnityRot(this System.Numerics.Quaternion self) {
            return new Quaternion(self.X, self.Y, self.Z, self.W);
        }

        public static Vector3 ToUnityVec(this System.Numerics.Vector3 self) {
            return new Vector3(self.X, self.Y, self.Z);
        }

    }

}