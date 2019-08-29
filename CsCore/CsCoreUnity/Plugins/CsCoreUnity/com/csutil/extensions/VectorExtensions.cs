using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.csutil {

    public static class VectorExtensions {

        public static Vector3 SetX(this Vector3 self, float x) { self.x = x; return self; }
        public static Vector3 SetY(this Vector3 self, float y) { self.y = y; return self; }
        public static Vector3 SetZ(this Vector3 self, float z) { self.z = z; return self; }

        public static Vector2 SetX(this Vector2 self, float x) { self.x = x; return self; }
        public static Vector2 SetY(this Vector2 self, float y) { self.y = y; return self; }

        public static float AngleTo(this Vector2 self, Vector2 to) {
            if (Vector3.Cross(self, to).z > 0) { return 360 - Vector2.Angle(self, to); }
            return Vector2.Angle(self, to);
        }

    }

}