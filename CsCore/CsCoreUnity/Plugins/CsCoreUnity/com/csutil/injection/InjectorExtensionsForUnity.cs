using System;
using com.csutil.injection;
using System.Linq;
using UnityEngine;

namespace com.csutil {

    public static class InjectorExtensionsForUnity {

        public const string DEFAULT_SINGLETON_NAME = "Singletons";

        public static T GetOrAddComponentSingleton<T>(this Injector self, object caller, string singletonsGoName = DEFAULT_SINGLETON_NAME) where T : Component {
            T x = self.Get<T>(caller, true);
            if (x == null) {
                var overrideExisting = x.IsDestroyed();  // override if there is an existing destroyed comp.
                x = GetOrAddComponentSingleton<T>(true, singletonsGoName);
                self.SetSingleton<T>(x, overrideExisting);
            }
            return x;
        }

        [Obsolete("Do not use, will be made private soon")]
        public static T GetOrAddComponentSingleton<T>(bool createIfNull, string singletonsGoName = DEFAULT_SINGLETON_NAME) where T : Component {
            var singletonsGo = GetOrAddSingletonGameObject(singletonsGoName);
            if (createIfNull) { return singletonsGo.GetOrAddChild("" + typeof(T)).GetOrAddComponent<T>(); }
            var t = singletonsGo.transform.Find("" + typeof(T));
            return t != null ? t.GetComponentV2<T>() : null;
        }

        public static GameObject GetOrAddSingletonGameObject(string goName) {
            GameObject go = GetSingletonGameObject(goName);
            if (go != null) { return go; }
            return new GameObject(goName);
        }

        /// <summary> Works better then GameObject.Find </summary>
        public static GameObject GetSingletonGameObject(string goName) {
            var go = GameObject.Find(goName);
            if (go != null) { return go; }
            var list = Resources.FindObjectsOfTypeAll<GameObject>().Filter(x => x.name == goName).ToList();
            if (!list.IsEmpty()) {
                go = list.Single(); // Must be exactly 1
                Log.d($"GameObject.Find could not find '{goName} but FindAllGOsInScene did'", go);
            }
            return go;
        }

    }

}