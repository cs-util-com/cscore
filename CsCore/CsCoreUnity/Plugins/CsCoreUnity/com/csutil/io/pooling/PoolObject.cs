using UnityEngine;

namespace com.csutil {
    
    public class PoolObject : MonoBehaviour {

        public IObjectPool<GameObject> pool;

        public bool Despawn() {
            pool.ThrowErrorIfNull("pool");
            return pool.Despawn(gameObject);
        }
        
    }
    
}