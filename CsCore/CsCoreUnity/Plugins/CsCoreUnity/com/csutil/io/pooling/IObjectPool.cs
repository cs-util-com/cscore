using System;
using UnityEngine;

namespace com.csutil {
    
    public interface IObjectPool<T> {
        Func<GameObject, T> onCreate { set; }
        T Spawn();
        bool Despawn(T objectToReturnToPool);
    }
    
}