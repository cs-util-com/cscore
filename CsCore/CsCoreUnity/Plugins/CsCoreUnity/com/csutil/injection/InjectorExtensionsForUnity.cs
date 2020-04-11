using com.csutil.injection;
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

        public static T GetOrAddComponentSingleton<T>(bool createIfNull, string singletonsGoName = DEFAULT_SINGLETON_NAME) where T : Component {
            var singletonsGo = GetOrAddGameObject(singletonsGoName);
            if (createIfNull) { return singletonsGo.GetOrAddChild("" + typeof(T)).GetOrAddComponent<T>(); }
            var t = singletonsGo.transform.Find("" + typeof(T));
            return t != null ? t.GetComponentV2<T>() : null;
        }

        public static GameObject GetOrAddGameObject(string gameObjectName) {
            var go = GameObject.Find(gameObjectName);
            if (go == null) { return new GameObject(gameObjectName); } else { return go; }
        }

    }

}
