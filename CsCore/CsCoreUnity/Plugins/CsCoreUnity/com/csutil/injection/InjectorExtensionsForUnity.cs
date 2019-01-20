using com.csutil.injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class InjectorExtensionsForUnity {

        private const string DEFAULT_SINGLETON_NAME = "Singletons";

        public static T GetOrAddComponentSingleton<T>(this Injector self, object caller, string singletonGo = DEFAULT_SINGLETON_NAME) where T : Component {
            var x = self.Get<T>(caller, true);
            if (x == null) {
                x = GetComponentSingleton<T>(true, singletonGo);
                self.SetSingleton(x);
            }
            return x;
        }

        public static T GetComponentSingleton<T>(bool createIfNull, string masterGo = DEFAULT_SINGLETON_NAME) where T : Component {
            var s = GetOrAddGameObject(masterGo);
            if (createIfNull) { return s.GetOrAddChild("" + typeof(T)).GetOrAddComponent<T>(); }
            var t = s.transform.Find("" + typeof(T));
            return t != null ? t.GetComponent<T>() : null;
        }

        private static GameObject GetOrAddGameObject(string gameObjectName) {
            var go = GameObject.Find(gameObjectName);
            if (go == null) { return new GameObject(gameObjectName); } else { return go; }
        }

    }

}
