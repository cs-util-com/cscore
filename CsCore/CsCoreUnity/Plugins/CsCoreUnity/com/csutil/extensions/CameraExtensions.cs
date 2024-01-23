using UnityEngine;

namespace com.csutil {
    
    public static class CameraExtensions {
        
        public static bool CanSee(this Camera self,GameObject gameObject) {
            self.ThrowErrorIfNull("camera");
            Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(self);
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (var c in colliders) {
                if (GeometryUtility.TestPlanesAABB(cameraPlanes, c.bounds)) {
                    return true;
                }
            }
            return false;
        }
        
    }
    
}