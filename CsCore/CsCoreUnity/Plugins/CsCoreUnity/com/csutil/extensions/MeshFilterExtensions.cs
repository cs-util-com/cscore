using UnityEngine;

namespace com.csutil {

    public static class PolygonCreatorV2 {

        public static void Create2dPolygonMesh(this MeshFilter self, Vector2[] polygonPoints) {
            var triangulator = self.gameObject.GetOrAddComponent<PolygonCollider2D>();
            self.mesh = triangulator.TriangulateMesh(polygonPoints);
        }

        public static Mesh TriangulateMesh(this PolygonCollider2D triangulator, Vector2[] polygonPoints) {
            var oldPose = triangulator.transform.CopyPose();
            // Set global pose to identity so that the triangulation is not affected by the local pose:
            triangulator.transform.position = Vector3.zero;
            triangulator.transform.rotation = Quaternion.identity;
            triangulator.transform.scale(Vector3.one);
            triangulator.points = polygonPoints;
#if UNITY_2022_1_OR_NEWER
            triangulator.useDelaunayMesh = true;
#endif
            var result = triangulator.CreateMesh(false, false);
            triangulator.transform.ApplyPose(oldPose);
            return result;
        }

    }

}