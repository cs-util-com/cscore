using System.Numerics;

namespace com.csutil {

    public static class Matrix4x4Extensions {

        public static Matrix4x4 Compose(Vector3 position, Quaternion rotation, Vector3 scale) {
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        public static Matrix4x4 Transpose(this Matrix4x4 self) { return Matrix4x4.Transpose(self); }

        public static bool Decompose(this Matrix4x4 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation) {
            return Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
        }

        public static Vector3 Transform(this Matrix4x4 transformationMatrix, Vector3 vecToTransform) { return Vector3.Transform(vecToTransform, transformationMatrix); }

    }

}