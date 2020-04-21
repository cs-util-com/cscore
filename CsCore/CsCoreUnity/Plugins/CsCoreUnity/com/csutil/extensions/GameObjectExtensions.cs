using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class GameObjectExtensions {

        /// <summary> Adds a child GameObject to the calling new parent GameObject </summary>
        public static GameObject AddChild(this GameObject parentGo, GameObject child, bool worldPositionStays = false, int siblingIndex = -1) {
            child.transform.SetParent(parentGo.transform, worldPositionStays); // add it to parent
            if (siblingIndex > -1) { child.transform.SetSiblingIndex(siblingIndex); }
            return child;
        }

        /// <summary> Used for lazy-initialization of a GameObject, combine with go.GetOrAddComponent </summary>
        public static GameObject GetOrAddChild(this GameObject parentGo, string childName) {
            var childGo = GetChild(parentGo, childName);
            if (childGo != null) { return childGo; } // child found, return it
            var newChild = new GameObject(childName); // no child found, create it
            newChild.transform.SetParent(parentGo.transform, false); // add it to parent
            return newChild;
        }

        public static GameObject GetChild(this GameObject self, string childName) { return self.transform.Find(childName)?.gameObject; }
        public static GameObject GetChild(this GameObject self, int index) { return self.transform.GetChild(index)?.gameObject; }

        /// <summary> For operations that will execute on all children use GetChildren() instead! </summary>
        public static IEnumerable<GameObject> GetChildrenIEnumerable(this GameObject self) { return self.transform.Cast<Transform>().Map(x => x.gameObject); }
        public static List<GameObject> GetChildren(this GameObject self) { return self.GetChildrenIEnumerable().ToList(); }

        public static int GetChildCount(this GameObject self) { return self.transform.childCount; }

        /// <summary> Unity returns a comp that pretents to be null so return actual null </summary>
        public static T GetComponentV2<T>(this GameObject self) where T : Component {
            var existingComp = self.GetComponent<T>();
            return existingComp == null ? null : existingComp;
        }

        /// <summary> Unity returns a comp that pretents to be null so return actual null </summary>
        public static T GetComponentV2<T>(this Component self) where T : Component {
            var existingComp = self.GetComponent<T>();
            return existingComp == null ? (T)(object)null : existingComp;
        }

        /// <summary> Used for lazy-initialization of a Mono, combine with go.GetOrAddChild </summary>
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component {
            var existingComp = self.GetComponentV2<T>();
            return existingComp == null ? self.AddComponent<T>() : existingComp;
        }

        public static bool HasComponent<T>(this GameObject self, out T existingComp) where T : Component {
            existingComp = self.GetComponentV2<T>();
            return existingComp != null;
        }

        /// <summary> Searches recursively upwards in all parents until a comp of type T is found </summary>
        public static T GetComponentInParents<T>(this GameObject gameObject) where T : Component {
            var comp = gameObject.GetComponentV2<T>();
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
        public static bool IsDestroyed(this UnityEngine.Object self) { return self == null && !ReferenceEquals(self, null); }

        public static bool Destroy(this UnityEngine.Object self, bool destroyNextFrame = false) {
            if (self == null) { return false; }
            try { if (destroyNextFrame) { UnityEngine.Object.Destroy(self); } else { GameObject.DestroyImmediate(self); } } catch { return false; }
            AssertV2.IsTrue(destroyNextFrame || self.IsDestroyed(), "gameObject was not destroyed");
            return true;
        }

        public static bool SetActiveV2(this GameObject self, bool active) {
            if (self == null) { return false; }
            self.SetActive(active);
            return true;
        }

        public static Bounds GetRendererBoundsOfAllChildren(this GameObject self) {
            Renderer[] renderers = self.GetComponentsInChildren<Renderer>();
            var bounds = renderers.First().bounds;
            foreach (Renderer renderer in renderers) { bounds.Encapsulate(renderer.bounds); }
            return bounds;
        }

        /// <summary> Combines the names of all parents with the GOs name "GO 1 -> Child 1 -> Abc" </summary>
        public static string FullQualifiedName(this GameObject self, string separator = " -> ") {
            var parent = self.GetParent();
            return (parent != null ? parent.FullQualifiedName() + separator : "") + self.name;
        }

    }

}
