using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class GameObjectExtensions {

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component {
            var existingComp = self.GetComponent<T>();
            return existingComp == null ? self.AddComponent<T>() : existingComp;
        }

        public static GameObject AddChild(this GameObject parentGo, GameObject child, bool worldPositionStays = false) {
            child.transform.SetParent(parentGo.transform, worldPositionStays); // add it to parent
            return child;
        }

        public static GameObject GetOrAddChild(this GameObject parentGo, string childName) {
            var childGo = parentGo.transform.Find(childName);
            if (childGo != null) { return childGo.gameObject; } // child found, return it
            var newChild = new GameObject(childName);        // no child found, create it
            newChild.transform.SetParent(parentGo.transform, false); // add it to parent
            return newChild;
        }

        public static GameObject GetParent(this GameObject child) {
            if (child == null || child.transform.parent == null) { return null; }
            return child.transform.parent.gameObject;
        }

        public static bool IsDestroyed(this GameObject self) {
            // == operator overloaded by gameObject but reference still exists
            return self == null && !ReferenceEquals(self, null);
        }

        public static bool Destroy(this GameObject self, bool destroyNextFrame = false) {
            if (self == null) { return false; }
            try { if (destroyNextFrame) { GameObject.Destroy(self); } else { GameObject.DestroyImmediate(self); } }
            catch { return false; }
            AssertV2.IsTrue(self.IsDestroyed(), "gameObject was not destroyed");
            return true;
        }

    }

}
