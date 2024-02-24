using UnityEngine;
using UnityEngine.Events;

namespace com.csutil {

    public class OnDestroyListener : MonoBehaviour {

        [System.Serializable]
        public class OnDestroyEvent : UnityEventV2 { }

        public OnDestroyEvent onDestroy = new OnDestroyEvent();

        private void OnDestroy() {
            if (onDestroy.IsNullOrEmptyV2()) { throw Log.e("OnDestroyListener had no listeners for " + gameObject.FullQualifiedName(), gameObject); }
            onDestroy.Invoke();
        }

    }

}