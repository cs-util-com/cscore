using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class GameObjectExtensions {

        /// <summary> Adds a child GameObject to the calling new parent GameObject </summary>
        public static GameObject AddChild(this GameObject parentGo, GameObject child, bool worldPositionStays = false) {
            child.transform.SetParent(parentGo.transform, worldPositionStays); // add it to parent
            return child;
        }

        /// <summary> Used for lazy-initialization of a GameObject, combine with go.GetOrAddComponent </summary>
        public static GameObject GetOrAddChild(this GameObject parentGo, string childName) {
            var childGo = parentGo.transform.Find(childName);
            if (childGo != null) { return childGo.gameObject; } // child found, return it
            var newChild = new GameObject(childName);        // no child found, create it
            newChild.transform.SetParent(parentGo.transform, false); // add it to parent
            return newChild;
        }

        /// <summary> Used for lazy-initialization of a Mono, combine with go.GetOrAddChild </summary>
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component {
            var existingComp = self.GetComponent<T>();
            return existingComp == null ? self.AddComponent<T>() : existingComp;
        }

        /// <summary> Searches recursively upwards in all parents until a comp of type T is found </summary>
        public static T GetComponentInParents<T>(this GameObject gameObject) where T : Component {
            var comp = gameObject.GetComponent<T>();
            if (comp != null) { return comp; }
            var parent = gameObject.GetParent();
            if (parent != null && parent != gameObject) { return parent.GetComponentInParents<T>(); }
            return null;
        }

        /// <summary> Returns the parent GameObject or null if top scene level is reached </summary>
        public static GameObject GetParent(this GameObject child) {
            if (child == null || child.transform.parent == null) { return null; }
            return child.transform.parent.gameObject;
        }

        /// <summary> Returns true if the GameObject is null because it was destroyed </summary>
        // == operator overloaded by gameObject but reference still exists:
        public static bool IsDestroyed(this GameObject self) { return self == null && !ReferenceEquals(self, null); }

        /// <summary> Returns true if the Component is null because it was destroyed </summary>
        public static bool IsDestroyed(this Component self) { return self == null && !ReferenceEquals(self, null); }

        public static bool Destroy(this GameObject self, bool destroyNextFrame = false) {
            if (self == null) { return false; }
            try { if (destroyNextFrame) { GameObject.Destroy(self); } else { GameObject.DestroyImmediate(self); } } catch { return false; }
            AssertV2.IsTrue(self.IsDestroyed(), "gameObject was not destroyed");
            return true;
        }

        public static bool SetActiveV2(this GameObject self, bool active) {
            if (self == null) { return false; }
            self.SetActive(active);
            return true;
        }

    }

}
