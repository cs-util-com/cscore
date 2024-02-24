using UnityEngine;

namespace com.csutil {

    public static class TransformExtensions {

        /// <summary> Set the global scale of an object </summary>
        public static void scale(this Transform transform, Vector3 globalScale) {
            transform.localScale = Vector3.one;
            var g = transform.lossyScale;
            transform.localScale = new Vector3(globalScale.x / g.x, globalScale.y / g.y, globalScale.z / g.z);
        }

        public static Transform ApplyPose(this Transform self, Transformation poseToApply) {
            self.localScale = poseToApply.localScale;
            self.localRotation = poseToApply.localRotation;
            self.localPosition = poseToApply.localPosition;
            return self;
        }

        public static Transformation CopyPose(this Transform self) {
            return new Transformation() {
                localPosition = self.localPosition,
                localRotation = self.localRotation,
                localScale = self.localScale
            };
        }

        public static Vector3 ToLocalPosition(this Transform self, Vector3 globalPosition) {
            return self.InverseTransformPoint(globalPosition);
        }
        
    }

    public class Transformation {
        public Vector3 localPosition = Vector3.zero;
        public Quaternion localRotation = Quaternion.identity;
        public Vector3 localScale = Vector3.one;
    }

}