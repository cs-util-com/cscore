using System;
using System.Numerics;

namespace com.csutil {

    public static class Matrix4x4Extensions {

        public static Matrix4x4 Compose(Vector3 position, Quaternion rotation, Vector3 scale) {
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        public static Matrix4x4 Transpose(this Matrix4x4 self) { return Matrix4x4.Transpose(self); }

        public static Matrix4x4 Inverse(this Matrix4x4 self) {
            Matrix4x4.Invert(self, out var res);
            return res;
        }

        public static bool Decompose(this Matrix4x4 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation) {
            return Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
        }

        public static Vector3 Transform(this Matrix4x4 transformationMatrix, Vector3 vecToTransform) { return Vector3.Transform(vecToTransform, transformationMatrix); }

        public static void Set(this ref Matrix4x4 self, int rowIndex, int columnIndex, float value) {
            if (rowIndex < 0 || rowIndex > 3 || columnIndex < 0 || columnIndex > 3) {
                throw new ArgumentOutOfRangeException("Row and column indices must be between 0 and 3 inclusive.");
            }
            switch (rowIndex) {
                case 0:
                    switch (columnIndex) {
                        case 0:
                            self.M11 = value;
                            break;
                        case 1:
                            self.M12 = value;
                            break;
                        case 2:
                            self.M13 = value;
                            break;
                        case 3:
                            self.M14 = value;
                            break;
                    }
                    break;
                case 1:
                    switch (columnIndex) {
                        case 0:
                            self.M21 = value;
                            break;
                        case 1:
                            self.M22 = value;
                            break;
                        case 2:
                            self.M23 = value;
                            break;
                        case 3:
                            self.M24 = value;
                            break;
                    }
                    break;
                case 2:
                    switch (columnIndex) {
                        case 0:
                            self.M31 = value;
                            break;
                        case 1:
                            self.M32 = value;
                            break;
                        case 2:
                            self.M33 = value;
                            break;
                        case 3:
                            self.M34 = value;
                            break;
                    }
                    break;
                case 3:
                    switch (columnIndex) {
                        case 0:
                            self.M41 = value;
                            break;
                        case 1:
                            self.M42 = value;
                            break;
                        case 2:
                            self.M43 = value;
                            break;
                        case 3:
                            self.M44 = value;
                            break;
                    }
                    break;
            }
        }

        public static float Get(this Matrix4x4 self, int rowIndex, int columnIndex) {
            if (rowIndex < 0 || rowIndex > 3 || columnIndex < 0 || columnIndex > 3) {
                throw new ArgumentOutOfRangeException("Row and column indices must be between 0 and 3 inclusive.");
            }
            switch (rowIndex) {
                case 0:
                    switch (columnIndex) {
                        case 0: return self.M11;
                        case 1: return self.M12;
                        case 2: return self.M13;
                        case 3: return self.M14;
                    }
                    break;
                case 1:
                    switch (columnIndex) {
                        case 0: return self.M21;
                        case 1: return self.M22;
                        case 2: return self.M23;
                        case 3: return self.M24;
                    }
                    break;
                case 2:
                    switch (columnIndex) {
                        case 0: return self.M31;
                        case 1: return self.M32;
                        case 2: return self.M33;
                        case 3: return self.M34;
                    }
                    break;
                case 3:
                    switch (columnIndex) {
                        case 0: return self.M41;
                        case 1: return self.M42;
                        case 2: return self.M43;
                        case 3: return self.M44;
                    }
                    break;
            }
            throw new InvalidOperationException("Invalid matrix indices.");
        }

        public static Vector4 GetRow(this Matrix4x4 ma, int rowIndex) {
            if (rowIndex < 0 || rowIndex > 3) {
                throw new ArgumentOutOfRangeException("Row index must be between 0 and 3 inclusive.");
            }
            switch (rowIndex) {
                case 0: return new Vector4(ma.M11, ma.M12, ma.M13, ma.M14);
                case 1: return new Vector4(ma.M21, ma.M22, ma.M23, ma.M24);
                case 2: return new Vector4(ma.M31, ma.M32, ma.M33, ma.M34);
                case 3: return new Vector4(ma.M41, ma.M42, ma.M43, ma.M44);
            }
            throw new InvalidOperationException("Invalid row index.");
        }

    }

}