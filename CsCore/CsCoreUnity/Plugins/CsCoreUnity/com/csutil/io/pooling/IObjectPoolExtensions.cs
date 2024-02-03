using UnityEngine;

namespace com.csutil {
    
    public static class IObjectPoolExtensions {

        public static IObjectPool<GameObject> UseTemplate(this IObjectPool<GameObject> self, GameObject template) {
            template.GetOrAddComponent<PoolObject>().pool = self;
            self.onCreate = (parent) => {
                var newInstance = parent.AddChild(UnityEngine.Object.Instantiate(template));
                newInstance.GetOrAddComponent<PoolObject>().pool = self;
                return newInstance;
            };
            return self;
        }

    }
    
}