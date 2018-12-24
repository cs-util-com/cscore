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

    }

}
