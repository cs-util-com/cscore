using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public static class GameObjectExtensions {

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component {
            var existingComp = self.GetComponent<T>();
            return existingComp == null ? self.AddComponent<T>() : existingComp;
        }

    }

}
