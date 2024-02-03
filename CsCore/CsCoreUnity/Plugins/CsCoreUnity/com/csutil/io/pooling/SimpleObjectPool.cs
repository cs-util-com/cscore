using System;
using UnityEngine;
using UnityEngine.Pool;

namespace com.csutil {

    public class SimpleObjectPool : MonoBehaviour, IObjectPool<GameObject> {

        public Func<GameObject, GameObject> onCreate { private get; set; }
        public ObjectPool<GameObject> Pool { get; private set; }

        private void OnEnable() {
            this.Pool = new ObjectPool<GameObject>(OnCreate, OnTakeFromPool, OnReturnToPool, null);
        }

        private GameObject OnCreate() {
            return onCreate(gameObject);
        }

        private void OnTakeFromPool(GameObject obj) {
            obj.transform.SetParent(null, false);
            obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            obj.SetActiveV2(true);
        }

        private void OnReturnToPool(GameObject obj) {
            obj.SetActive(false);
            obj.transform.SetParent(transform, false);
        }

        public GameObject Spawn() {
            return Pool.Get();
        }

        public bool Despawn(GameObject objectToReturnToPool) {
            Pool.Release(objectToReturnToPool);
            return true;
        }

    }

}