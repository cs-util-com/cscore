using UnityEngine;

namespace com.csutil {

    public static class PolygonCreatorV2 {

        public static void Create2dPolygonMesh(this MeshFilter self, Vector2[] polygonPoints) {
            var triangulator = self.gameObject.GetOrAddComponent<PolygonCollider2D>();
            self.mesh = triangulator.TriangulateMesh(polygonPoints);
        }

        public static Mesh TriangulateMesh(this PolygonCollider2D triangulator, Vector2[] polygonPoints) {
            triangulator.points = polygonPoints;
            triangulator.useDelaunayMesh = true;
            return triangulator.CreateMesh(false, false);
        }

    }

}